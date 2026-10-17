using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace WeaveUI.Samples.BuilderThemes
{
    [RequireComponent(typeof(ScreenHost))]
    public sealed class BuilderThemesSample : MonoBehaviour
    {
        /// <summary>Where the Basic Menu sample keeps its skin of Kenney's UI art.</summary>
        const string KenneySkinPath = "WeaveUISamples/BasicMenu/PlaceholderSkin/PlaceholderSkin";

        /// <summary>Where Tools > Weave UI Samples > Build Synty Fantasy Menus Skin writes its skin.</summary>
        const string SyntySkinPath = "WeaveUISynty/SyntyFantasySkin";

        [Tooltip("Any UISkin, listed as Kenney. If empty the Basic Menu sample's skin is used.")]
        [SerializeField] UISkin _kenneySkin;

        [Tooltip("Any UISkin, listed as Synty. If empty the skin built from your Synty pack is used.")]
        [SerializeField] UISkin _syntySkin;

        private readonly Dictionary<string, UISkin> _skins = new();
        private ScreenHost _host;
        private CharacterViewModel _character;
        private StyleSheet _ember;

        void Start()
        {
            _host = GetComponent<ScreenHost>();
            _ember = Resources.Load<StyleSheet>("WeaveUISamples/BuilderThemes/EmberPalette");
            AddSkin("Kenney", _kenneySkin != null ? _kenneySkin : Resources.Load<UISkin>(KenneySkinPath));
            AddSkin("Synty", _syntySkin != null ? _syntySkin : Resources.Load<UISkin>(SyntySkinPath));
            _character = new CharacterViewModel { SkinChoices = new[] { "None" }.Concat(_skins.Keys).ToArray() };
            _character.propertyChanged += (_, e) =>
            {
                if (e.propertyName == nameof(CharacterViewModel.Theme) || e.propertyName == nameof(CharacterViewModel.Skin))
                {
                    ApplyTheme();
                }
            };
            _host.Screens.Push(new CharacterSheetScreen(_character));
        }

        void ApplyTheme()
        {
            var theme = _host.Theme;
            theme.Mode = _character.Theme == ThemeChoice.Light ? ThemeMode.Light : ThemeMode.Dark;
            theme.Palette = _character.Theme == ThemeChoice.Ember ? _ember : null;
            theme.Skin = _skins.TryGetValue(_character.Skin, out var skin) ? skin : null;
        }

        void AddSkin(string name, UISkin skin)
        {
            if (skin != null)
            {
                _skins[name] = skin;
            }
        }
    }
}
