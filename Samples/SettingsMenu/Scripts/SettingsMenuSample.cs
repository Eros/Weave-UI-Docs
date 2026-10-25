using UnityEngine;

namespace Nerdova.WeaveUI.Samples.SettingsMenu
{
    /// <summary>
    /// The sample's only component. Two settings act on the UI itself while you edit them: Light theme switches the
    /// palette and Reduce motion turns off screen transitions. Everything else is just data for your game to read.
    /// </summary>
    [RequireComponent(typeof(ScreenHost))]
    public sealed class SettingsMenuSample : MonoBehaviour
    {
        ScreenHost _host;
        SettingsViewModel _settings;

        void Start()
        {
            _host = GetComponent<ScreenHost>();
            _settings = new SettingsViewModel();
            _settings.propertyChanged += (_, e) =>
            {
                if (e.propertyName == nameof(SettingsViewModel.LightTheme) || e.propertyName == nameof(SettingsViewModel.ReduceMotion))
                    ApplyToUi();
            };
            ApplyToUi();
            _host.Screens.Push(new SettingsScreen(_settings));
        }

        void ApplyToUi()
        {
            _host.Theme.Mode = _settings.LightTheme ? ThemeMode.Light : ThemeMode.Dark;
            _host.Screens.DefaultTransition = ScreenTransition.Fade;
            _host.Screens.ReduceMotion = _settings.ReduceMotion;
        }
    }
}
