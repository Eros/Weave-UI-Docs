using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace WeaveUI.Samples.GameHud
{
    /// <summary>One hotbar ability or consumable. The slot template (HotbarSlot.uxml) binds to its properties.</summary>
    public sealed class HotbarSlot : ViewModel
    {
        private int _count;
        private float _cooldown;

        /// <param name="count">Uses for a consumable (it runs out); 0 for an ability (it never does).</param>
        public HotbarSlot(string name, string icon, float cooldownSeconds, int count = 0)
        {
            Name = name;
            CooldownSeconds = cooldownSeconds;
            IsConsumable = count > 0;
            _count = count;
            var texture = Resources.Load<Texture2D>($"WeaveUISamples/GameHud/Icons/{icon}");
            IconBackground = texture != null ? new StyleBackground(texture) : new StyleBackground(StyleKeyword.None);
        }

        [CreateProperty]
        public string Name { get; }

        [CreateProperty]
        public StyleBackground IconBackground { get; }

        public float CooldownSeconds { get; }

        [CreateProperty]
        public int Count
        {
            get => _count;
            set
            {
                if (SetProperty(ref _count, value))
                {
                    Notify(nameof(CountText));
                }
            }
        }

        [CreateProperty]
        public string CountText => _count > 0 ? _count.ToString() : "";

        public bool IsConsumable { get; }

        public bool IsReady => _cooldown <= 0 && (!IsConsumable || _count > 0);

        /// <summary>Seconds until the slot can be used again.</summary>
        public float Cooldown
        {
            get => _cooldown;
            set
            {
                if (SetProperty(ref _cooldown, Mathf.Max(0, value)))
                {
                    Notify(nameof(CooldownHeight));
                }
            }
        }

        /// <summary>The dark overlay drains from the top as the cooldown runs out.</summary>
        [CreateProperty]
        public StyleLength CooldownHeight => Length.Percent(CooldownSeconds > 0 ? 100 * _cooldown / CooldownSeconds : 0);
    }
}
