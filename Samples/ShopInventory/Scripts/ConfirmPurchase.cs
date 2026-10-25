using System;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Nerdova.WeaveUI.Samples.ShopInventory
{
    /// <summary>
    /// "Are you sure?" for expensive purchases. A <see cref="ModalScreen{TViewModel}"/> whose data source is the item,
    /// so ConfirmPurchase.uxml binds the icon, name and price like any other screen.
    /// </summary>
    [ScreenLayout("WeaveUISamples/ShopInventory/ConfirmPurchase")]
    public sealed class ConfirmPurchase : ModalScreen<Item>
    {
        readonly Action _onConfirm;

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
