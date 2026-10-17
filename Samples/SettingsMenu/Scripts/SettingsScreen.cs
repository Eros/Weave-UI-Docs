using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;
using static WeaveUI.Ui;

namespace WeaveUI.Samples.SettingsMenu
{
    [ScreenLayout("WeaveUISamples/SettingsMenu/Settings")]
    public sealed class SettingsScreen : Screen<SettingsViewModel>
    {
        private KeyBinding _listening;

        public SettingsScreen(SettingsViewModel viewModel) : base(viewModel) { }

        protected override void OnCreated()
        {
            Root.Q("bindings").Children(ViewModel.Bindings, BindingRow);
            // Trickle down so the key reaches us before the focused button treats Space or Enter as a click.
            Root.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        }

        protected override void OnShow() => Root.Q<Slider>("master").Focus();

        /// <summary>Escape cancels a rebind instead of leaving the screen.</summary>
        protected override bool OnBack()
        {
            if (_listening == null)
            {
                return false;
            }

            StopListening();
            return true;
        }

        /// <summary>click="AskDefaults": not on the view model, so the screen's method is used.</summary>
        [Preserve]
        void AskDefaults() => Manager.Push(new ConfirmDialog(
            "Restore defaults?",
            "Every setting on every tab goes back to its default. Nothing is saved until you press Apply.",
            "Restore defaults",
            ViewModel.RestoreDefaults));

        VisualElement BindingRow(KeyBinding binding)
        {
            var key = Button(binding.KeyText)
                .Class("keycap")
                .DataSource(binding)
                .Bind(nameof(KeyBinding.KeyText), "text")
                .OnClick(() => Listen(binding));
            binding.propertyChanged += (_, _) => key.EnableInClassList("keycap--listening", binding.Listening);

            return Row().Class("setting-row").Children(
                Text(binding.Action).Class("setting-row__name flex-1"),
                key);
        }

        void Listen(KeyBinding binding)
        {
            StopListening();
            _listening = binding;
            binding.Listening = true;
        }

        void StopListening()
        {
            if (_listening != null)
            {
                _listening.Listening = false;
            }
            _listening = null;
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            if (_listening == null || evt.keyCode == KeyCode.None || evt.keyCode == KeyCode.Escape)
            {
                return;
            }

            _listening.Key = evt.keyCode;
            StopListening();
            evt.StopPropagation();
        }
    }
}
