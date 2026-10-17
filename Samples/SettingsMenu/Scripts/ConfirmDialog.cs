using System;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace WeaveUI.Samples.SettingsMenu
{
    [ScreenLayout("WeaveUISamples/SettingsMenu/Confirm")]
    public sealed class ConfirmDialog : ModalScreen
    {
        private readonly string _title;
        private readonly string _body;
        private readonly string _confirm;
        private readonly Action _onConfirm;

        public ConfirmDialog(string title, string body, string confirm, Action onConfirm) =>
            (_title, _body, _confirm, _onConfirm) = (title, body, confirm, onConfirm);

        protected override void OnCreated()
        {
            Root.Q<Label>("title").text = _title;
            Root.Q<Label>("body").text = _body;
            Root.Q<Button>("confirm").text = _confirm;
        }

        protected override void OnShow() => Root.Q<Button>("cancel").Focus();

        [Preserve]
        void Confirm()
        {
            Close();
            _onConfirm?.Invoke();
        }

        [Preserve]
        void Cancel() => Close();
    }
}
