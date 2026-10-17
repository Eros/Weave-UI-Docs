using UnityEngine;

namespace WeaveUI.Samples.SettingsMenu
{
    [RequireComponent(typeof(ScreenHost))]
    public sealed class SettingsMenuSample : MonoBehaviour
    {
        private ScreenHost _host;
        private SettingsViewModel _settings;

        void Start()
        {
            _host = GetComponent<ScreenHost>();
            _settings = new SettingsViewModel();
            _settings.propertyChanged += (_, e) =>
            {
                if (e.propertyName == nameof(SettingsViewModel.LightTheme) || e.propertyName == nameof(SettingsViewModel.ReduceMotion))
                {
                    ApplyToUi();
                }
            };
            ApplyToUi();
            _host.Screens.Push(new SettingsScreen(_settings));
        }

        void ApplyToUi()
        {
            _host.Theme.Mode = _settings.LightTheme ? ThemeMode.Light : ThemeMode.Dark;
            _host.Screens.DefaultTransition = _settings.ReduceMotion ? ScreenTransition.None : ScreenTransition.Fade;
        }
    }
}
