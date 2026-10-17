using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static WeaveUI.Ui;

namespace WeaveUI.Samples.BuilderThemes
{
    public sealed class CharacterSheetScreen : Screen<CharacterViewModel>
    {
        private static readonly string[] Stances = { "Balanced", "Aggressive", "Defensive" };
        private readonly Dictionary<ThemeChoice, WeaveButton> _segments = new();

        public CharacterSheetScreen(CharacterViewModel viewModel) : base(viewModel) { }

        protected override VisualElement CreateRoot()
        {
            var root = El("sheet").Class("sheet flex-1 items-center justify-center").Children(
                Panel("card").Class("sheet__card").Children(
                    Header(),
                    Row().Class("items-stretch gap-5").Children(
                        Attributes(),
                        Derived(),
                        Preferences()),
                    Footer()));
            root.styleSheets.Add(Resources.Load<StyleSheet>("WeaveUISamples/BuilderThemes/CharacterSheet"));
            return root;
        }

        protected override void OnCreated()
        {
            ViewModel.propertyChanged += OnChanged;
            UpdateSegments();
        }

        protected override void OnClosed() => ViewModel.propertyChanged -= OnChanged;

        // Crest, name, theme switcher

        VisualElement Header() =>
            Row().Class("items-center mb-6").Children(
                El().Class("crest mr-4").Children(Text("A").Class("crest__initial")),
                Column().Class("flex-1").Children(
                    Title("Ada Thornfield").Class("mb-0"),
                    Text().Bind(nameof(CharacterViewModel.Subtitle)).Class("muted text-sm")),
                Row("themes").Class("segments mr-4").Children(
                    Segment(ThemeChoice.Dark),
                    Segment(ThemeChoice.Light),
                    Segment(ThemeChoice.Ember)),
                Dropdown("Skin", ViewModel.SkinChoices)
                    .Class("compact-field")
                    .Bind(nameof(CharacterViewModel.Skin))
                    .Tooltip("Swaps the controls' look for 9-slice sprites; the markup doesn't change"));

        WeaveButton Segment(ThemeChoice choice)
        {
            var button = Button(choice.ToString()).Size("sm").Class("segment").OnClick(() => ViewModel.Theme = choice);
            _segments[choice] = button;
            return button;
        }

        // Columns

        VisualElement Attributes() =>
            Column("attributes").Class("sheet-column").Children(
                Row().Class("items-center justify-between mb-3").Children(
                    Text("ATTRIBUTES").Class("section-heading"),
                    Text().Bind(nameof(CharacterViewModel.PointsText)).Class("points")),
                Column().Children(ViewModel.Stats, StatRow));

        /// <summary>One attribute; the row's data source is the Stat, so its bindings name Stat properties.</summary>
        VisualElement StatRow(Stat stat) =>
            Column().Class("mb-4").DataSource(stat).Children(
                Row().Class("items-center justify-between mb-1").Children(
                    Text(stat.Name).Class("stat__name"),
                    Text().Bind(nameof(Stat.ValueText)).Class("stat__value")),
                Row().Class("items-center").Children(
                    El().Class("stat__bar flex-1 mr-3").Children(
                        El().Class("stat__fill").Bind(nameof(Stat.Fill), "style.width")),
                    Button("−").Size("sm").Variant("secondary").Class("stat__step mr-1")
                        .Bind(nameof(Stat.CanDecrease)).OnClick(() => ViewModel.Decrease(stat)),
                    Button("+").Size("sm").Variant("primary").Class("stat__step")
                        .Bind(nameof(Stat.CanIncrease)).OnClick(() => ViewModel.Increase(stat))));

        VisualElement Derived() =>
            Column("derived").Class("sheet-column").Children(
                Text("DERIVED").Class("section-heading mb-3"),
                Figure("Health", nameof(CharacterViewModel.HealthText)),
                Figure("Mana", nameof(CharacterViewModel.ManaText)),
                Figure("Critical chance", nameof(CharacterViewModel.CritText)),
                Figure("Carry weight", nameof(CharacterViewModel.CarryText)),
                Text("RESISTANCES").Class("section-heading mt-5 mb-2"),
                ProgressBar("Fire  35%", 35),
                ProgressBar("Frost  60%", 60),
                ProgressBar("Poison  20%", 20).Variant("danger"));

        static VisualElement Figure(string label, string path) =>
            Row().Class("figure").Children(
                Text(label).Class("flex-1 muted"),
                Text().Bind(path).Class("figure__value"));

        VisualElement Preferences() =>
            Column("preferences").Class("sheet-column").Children(
                Text("PREFERENCES").Class("section-heading mb-3"),
                TextField("Epithet").Bind(nameof(CharacterViewModel.Epithet)),
                Dropdown("Stance", Stances).Bind(nameof(CharacterViewModel.Stance)),
                Slider("Companion distance", 1, 10).Bind(nameof(CharacterViewModel.CompanionDistance)),
                Toggle("Auto-loot").Bind(nameof(CharacterViewModel.AutoLoot))
                    .Tooltip("Pick up gold and crafting materials on the way past"));

        VisualElement Footer() =>
            Row().Class("items-center mt-6").Children(
                Text().Bind(nameof(CharacterViewModel.Status)).Class("flex-1 status"),
                Button("Refund points").Variant("ghost").Class("mr-2").OnClick(() => ViewModel.ResetPoints()),
                Button("Confirm").Variant("primary").Bind(nameof(CharacterViewModel.HasSpent)).OnClick(() => ViewModel.Confirm()));

        // Theme switcher state

        void OnChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            if (e.propertyName == nameof(CharacterViewModel.Theme))
            {
                UpdateSegments();
            }
        }

        /// <summary>The active theme's button becomes a primary button; the others are ghosts.</summary>
        void UpdateSegments()
        {
            foreach (var (choice, button) in _segments)
            {
                button.Variant(choice == ViewModel.Theme ? "primary" : "ghost");
            }
        }
    }
}
