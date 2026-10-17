using System;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace WeaveUI.Samples.ShopInventory
{
    [ScreenLayout("WeaveUISamples/ShopInventory/ConfirmPurchase")]
    public sealed class ConfirmPurchase : ModalScreen<Item>
    {
        private readonly Action _onConfirm;

        public ConfirmPurchase(Item item, Action onConfirm) : base(item) => _onConfirm = onConfirm;

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
