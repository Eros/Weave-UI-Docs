using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace WeaveUI.Samples.GameHud
{
    /// <summary>A short message in the HUD's toast stack. Tone picks its accent colour (toast--gold, toast--level, ...).</summary>
    public readonly struct Toast
    {
        public Toast(string title, string body, string tone) => (Title, Body, Tone) = (title, body, tone);

        public string Title { get; }
        public string Body { get; }
        public string Tone { get; }
    }

    public sealed class Quest
    {
        public Quest(string title, string step, bool done = false) => (Title, Step, Done) = (title, step, done);

        public string Title { get; }
        public string Step { get; }
        public bool Done { get; }
    }

    public sealed class HudViewModel : ViewModel
    {
        const float MaxHealth = 120;
        const float MaxMana = 80;
        const float ManaPerSecond = 2.5f;

        private float _health = 104;
        private float _mana = 45;
        private float _xp = 68;
        private int _level = 12;
        private int _gold = 1284;
        private int _gems = 12;

        /// <summary>Raised for anything worth a toast; the screen turns it into an element.</summary>
        public event Action<Toast> Toasted;

        [CreateProperty]
        public string PlayerName => "Ada";

        [CreateProperty]
        public string ClassName => "Wayfinder";

        [CreateProperty]
        public string LevelText => _level.ToString();

        [CreateProperty]
        public float Health
        {
            get => _health;
            private set
            {
                if (!SetProperty(ref _health, Mathf.Clamp(value, 0, MaxHealth)))
                {
                    return;
                }

                Notify(nameof(HealthFill));
                Notify(nameof(HealthText));
            }
        }

        /// <summary>Bound to the width of both the fill and the white "damage taken" bar, which follows after a delay (see Hud.uss).</summary>
        [CreateProperty]
        public StyleLength HealthFill => Length.Percent(100 * _health / MaxHealth);

        [CreateProperty]
        public string HealthText => $"{_health:0} / {MaxHealth:0}";

        [CreateProperty]
        public float Mana
        {
            get => _mana;
            private set
            {
                if (!SetProperty(ref _mana, Mathf.Clamp(value, 0, MaxMana)))
                {
                    return;
                }

                Notify(nameof(ManaFill));
                Notify(nameof(ManaText));
            }
        }

        [CreateProperty]
        public StyleLength ManaFill => Length.Percent(100 * _mana / MaxMana);

        [CreateProperty]
        public string ManaText => $"{_mana:0} / {MaxMana:0}";

        /// <summary>0-100, bound to a weave:ProgressBar.</summary>
        [CreateProperty]
        public float Xp
        {
            get => _xp;
            private set
            {
                if (SetProperty(ref _xp, value))
                {
                    Notify(nameof(XpTitle));
                }
            }
        }

        [CreateProperty]
        public string XpTitle => $"{_xp:0}% to level {_level + 1}";

        [CreateProperty]
        public string GoldText => _gold.ToString("N0");

        [CreateProperty]
        public string GemsText => _gems.ToString("N0");

        [CreateProperty]
        public string Heading => "NE  38°";

        [CreateProperty]
        public string Prompt => "Talk to Mirela";

        /// <summary>Eight slots, null for an empty one. The hotbar grid binds its ItemsSource here.</summary>
        [CreateProperty]
        public List<HotbarSlot> Hotbar { get; } = new()
        {
            new HotbarSlot("Ashen Blade", "sword", 0.8f),
            new HotbarSlot("Kite Shield", "shield", 6f),
            new HotbarSlot("Healing Draught", "potion", 3f, count: 4),
            new HotbarSlot("Willow Longbow", "bow", 1.5f),
            new HotbarSlot("Scroll of Return", "scroll", 30f, count: 2),
            null,
            new HotbarSlot("Starlit Sapphire", "gem", 12f, count: 1),
            null,
        };

        public IReadOnlyList<Quest> Quests { get; } = new[]
        {
            new Quest("The Lighthouse Keeper", "Find Mirela at the harbour"),
            new Quest("Salt and Iron", "Collect sea iron  3 / 5"),
            new Quest("A Quiet Arrival", "Reach the harbour town", done: true),
        };

        /// <summary>Recent combat and loot lines; Unity's rich text tags colour them.</summary>
        public IReadOnlyList<string> Log { get; } = new[]
        {
            "<color=#94a3b8>You enter</color> <b>Saltmarsh Harbour</b>",
            "<color=#fda4af>Tide Crawler</color> hits you for <color=#fda4af>14</color>",
            "You hit <color=#fda4af>Tide Crawler</color> for <color=#fde68a>32</color> (critical)",
            "<color=#6ee7b7>Looted</color> Sea Iron ×2",
        };

        /// <summary>Advances cooldowns and mana regeneration. Call it once per frame.</summary>
        public void Tick(float deltaTime)
        {
            Mana += ManaPerSecond * deltaTime;
            foreach (var slot in Hotbar)
            {
                if (slot != null && slot.Cooldown > 0)
                {
                    slot.Cooldown -= deltaTime;
                }
            }
        }

        public void TakeDamage(float amount) => Health -= amount;

        public void GainGold(int amount)
        {
            _gold += amount;
            Notify(nameof(GoldText));
            Toasted?.Invoke(new Toast($"+{amount} gold", "Found in a weathered chest", "gold"));
        }

        public void GainXp(float amount)
        {
            var xp = _xp + amount;
            if (xp < 100)
            {
                Xp = xp;
                return;
            }

            _level++;
            Notify(nameof(LevelText));
            Xp = xp - 100;
            Health = MaxHealth;
            Toasted?.Invoke(new Toast($"Level {_level}!", "Health restored, 1 talent point to spend", "level"));
        }

        /// <summary>Uses a hotbar slot: a potion heals, everything else just starts its cooldown.</summary>
        public void Use(int index)
        {
            var slot = index >= 0 && index < Hotbar.Count ? Hotbar[index] : null;
            if (slot == null)
            {
                return;
            }

            if (!slot.IsReady)
            {
                Toasted?.Invoke(new Toast(slot.Name, slot.Cooldown > 0 ? $"Ready in {slot.Cooldown:0.0}s" : "None left", "muted"));
                return;
            }

            slot.Cooldown = slot.CooldownSeconds;
            if (slot.IsConsumable)
            {
                slot.Count--;
            }
            if (slot.Name == "Healing Draught")
            {
                Health += 30;
            }
            if (slot.Name == "Starlit Sapphire")
            {
                _gems++;
                Notify(nameof(GemsText));
                Toasted?.Invoke(new Toast("Sapphire socketed", "+1 gem", "loot"));
            }
        }
    }
}
