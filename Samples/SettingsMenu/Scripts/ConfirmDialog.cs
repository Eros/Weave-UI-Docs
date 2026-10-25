using System;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Nerdova.WeaveUI.Samples.SettingsMenu
{
    /// <summary>
    /// A reusable yes/no dialog. As a <see cref="ModalScreen"/> it sits over the current screen with a backdrop:
    /// clicking the backdrop, Escape or Cancel closes it; Confirm runs the action, then closes it.
    /// </summary>
    [ScreenLayout("WeaveUISamples/SettingsMenu/Confirm")]
    public sealed class ConfirmDialog : ModalScreen
    {
        readonly string _title;
        readonly string _body;
        readonly string _confirm;
        readonly Action _onConfirm;

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
