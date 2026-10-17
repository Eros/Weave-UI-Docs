using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEngine.UIElements;

namespace WeaveUI.Samples.BuilderThemes
{
    public enum ThemeChoice
    {
        Dark,
        Light,
        Ember,
    }

    /// <summary>One attribute you can spend points on. Each stat row binds to its own Stat.</summary>
    public sealed class Stat : ViewModel
    {
        const int Max = 20;

        private readonly CharacterViewModel _owner;
        private int _value;

        public Stat(CharacterViewModel owner, string name, int value)
        {
            _owner = owner;
            Name = name;
            Base = _value = value;
        }

        [CreateProperty] public string Name { get; }
        public int Base { get; private set; }

        public int Value
        {
            get => _value;
            set
            {
                if (!SetProperty(ref _value, value))
                {
                    return;
                }

                Notify(nameof(ValueText));
                Notify(nameof(Fill));
                Notify(nameof(CanDecrease));
            }
        }

        [CreateProperty] public string ValueText => _value == Base ? $"{_value}" : $"{_value}  (+{_value - Base})";
        [CreateProperty] public StyleLength Fill => Length.Percent(100f * _value / Max);
        [CreateProperty] public bool CanIncrease => _owner.Points > 0 && _value < Max;
        [CreateProperty] public bool CanDecrease => _value > Base;

        /// <summary>Makes the current value the new floor: points spent on it can no longer be taken back.</summary>
        public void Commit()
        {
            Base = _value;
            Notify(nameof(ValueText));
            Notify(nameof(CanDecrease));
        }

        internal void PointsChanged() => Notify(nameof(CanIncrease));
    }

    public sealed class CharacterViewModel : ViewModel
    {
        private int _points = 3;
        private string _stance = "Balanced";
        private bool _autoLoot = true;
        private float _companionDistance = 4;
        private string _epithet = "Lantern-bearer";
        private string _status = "Spend your points, then confirm. The buttons above restyle every control.";
        private ThemeChoice _theme;
        private string _skin = "None";

        public CharacterViewModel()
        {
            Stats = new[]
            {
                new Stat(this, "Strength", 9),
                new Stat(this, "Agility", 14),
                new Stat(this, "Intellect", 11),
                new Stat(this, "Spirit", 7),
            };
            foreach (var stat in Stats)
            {
                stat.propertyChanged += (_, e) =>
                {
                    if (e.propertyName == nameof(Stat.Value))
                    {
                        NotifyDerived();
                    }
                };
            }
        }

        public IReadOnlyList<Stat> Stats { get; }

        Stat Strength => Stats[0];
        Stat Agility => Stats[1];
        Stat Intellect => Stats[2];

        public int Points
        {
            get => _points;
            private set
            {
                if (!SetProperty(ref _points, value))
                {
                    return;
                }

                Notify(nameof(PointsText));
                Notify(nameof(HasSpent));
                foreach (var stat in Stats)
                {
                    stat.PointsChanged();
                }
            }
        }

        [CreateProperty] public string Subtitle => $"Level 12 Wayfinder, {_epithet}";
        [CreateProperty] public string PointsText => _points == 1 ? "1 point to spend" : $"{_points} points to spend";

        /// <summary>Confirm is enabled once a point has been spent.</summary>
        [CreateProperty] public bool HasSpent => Stats.Any(s => s.Value != s.Base);

        // Derived stats

        [CreateProperty] public string HealthText => $"{80 + 12 * Strength.Value}";
        [CreateProperty] public string ManaText => $"{40 + 10 * Intellect.Value}";
        [CreateProperty] public string CritText => $"{5 + 1.5f * Agility.Value:0.#}%";
        [CreateProperty] public string CarryText => $"{40 + 5 * Strength.Value} kg";

        // Preferences

        [CreateProperty] public string Stance { get => _stance; set => SetProperty(ref _stance, value); }
        [CreateProperty] public bool AutoLoot { get => _autoLoot; set => SetProperty(ref _autoLoot, value); }
        [CreateProperty] public float CompanionDistance { get => _companionDistance; set => SetProperty(ref _companionDistance, value); }

        [CreateProperty]
        public string Epithet
        {
            get => _epithet;
            set
            {
                if (SetProperty(ref _epithet, value))
                {
                    Notify(nameof(Subtitle));
                }
            }
        }

        [CreateProperty]
        public string Status
        {
            get => _status;
            private set => SetProperty(ref _status, value);
        }

        // Theme and skin settings

        public ThemeChoice Theme
        {
            get => _theme;
            set => SetProperty(ref _theme, value);
        }

        /// <summary>"None" plus the skins the sample found; the skin dropdown lists these.</summary>
        public IReadOnlyList<string> SkinChoices { get; set; } = new[] { "None" };

        [CreateProperty]
        public string Skin
        {
            get => _skin;
            set => SetProperty(ref _skin, value);
        }

        // Actions

        public void Increase(Stat stat)
        {
            if (!stat.CanIncrease)
            {
                return;
            }

            stat.Value++;
            Points--;
        }

        public void Decrease(Stat stat)
        {
            if (!stat.CanDecrease)
            {
                return;
            }

            stat.Value--;
            Points++;
        }

        public void ResetPoints()
        {
            foreach (var stat in Stats)
            {
                Points += stat.Value - stat.Base;
                stat.Value = stat.Base;
            }

            Status = "Points refunded.";
        }

        public void Confirm()
        {
            foreach (var stat in Stats)
            {
                stat.Commit();
            }
            Notify(nameof(HasSpent));
            Status = "Attributes confirmed.";
        }

        void NotifyDerived()
        {
            Notify(nameof(HealthText));
            Notify(nameof(ManaText));
            Notify(nameof(CritText));
            Notify(nameof(CarryText));
            Notify(nameof(HasSpent));
        }
    }
}
