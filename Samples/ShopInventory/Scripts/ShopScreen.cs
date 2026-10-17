using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace WeaveUI.Samples.ShopInventory
{
    [ScreenLayout("WeaveUISamples/ShopInventory/Shop")]
    public sealed class ShopScreen : Screen<ShopViewModel>
    {
        private WeaveGrid _vendor;
        private WeaveGrid _bag;

        public ShopScreen(ShopViewModel viewModel) : base(viewModel) { }

        protected override void OnCreated()
        {
            _vendor = Root.Q<WeaveGrid>("vendor");
            _vendor.CellTemplate = Resources.Load<VisualTreeAsset>("WeaveUISamples/ShopInventory/StockCard");
            _vendor.DropRule = drag => drag.Source == _bag ? DropOutcome.Merge : DropOutcome.Reject;

            _bag = Root.Q<WeaveGrid>("bag");
            _bag.CellTemplate = Resources.Load<VisualTreeAsset>("WeaveUISamples/ShopInventory/BagItem");
            _bag.Occupancy = ViewModel.Bag;
            _bag.ItemSize = item => ((Item)item).Size;
            _bag.DropRule = drag => drag.Source == _vendor && !ViewModel.CanAfford((Item)drag.SourceItem) ? DropOutcome.Reject : drag.Outcome;
        }

        protected override void OnShow() => _vendor.Focus();

        [OnCellClick("vendor")]
        void SelectStock(Item item) => ViewModel.Select(item, fromStock: true);

        [OnCellClick("bag")]
        void SelectOwned(Item item) => ViewModel.Select(item, fromStock: false);

        [OnCellClick("vendor", Clicks = 2)]
        void QuickBuy(Item item) => Buy(item);

        /// <summary>A drop on the bag: buy from the vendor, or move within the bag. The slot is the item's new top-left.</summary>
        [OnDrop("bag")]
        void DroppedInBag(GridDrag drag, [Slot] int slot)
        {
            var item = (Item)drag.SourceItem;
            var (x, y) = (slot % ViewModel.Bag.Width, slot / ViewModel.Bag.Width);
            if (drag.Source == _vendor)
            {
                ViewModel.Buy(item, x, y);
            }
            else
            {
                ViewModel.Move(item, x, y);
            }
        }

        [OnDrop("vendor")]
        void DroppedOnVendor([Source] Item item) => ViewModel.Sell(item);

        /// <summary>A rejected drop (no room, or not enough gold) snaps back; say why.</summary>
        [OnDragCancel("bag")]
        [OnDragCancel("vendor")]
        void Cancelled(Item item) => ViewModel.ExplainRejected(item);

        /// <summary>click="Act" on the details panel's button.</summary>
        [Preserve]
        void Act()
        {
            var item = ViewModel.Selected;
            if (item == null)
            {
                return;
            }

            if (ViewModel.SelectedIsStock)
            {
                Buy(item);
            }
            else
            {
                ViewModel.Sell(item);
            }
        }

        /// <summary>Expensive purchases go through a modal confirmation first.</summary>
        void Buy(Item item)
        {
            if (item.Price < ShopViewModel.ConfirmAbove || !ViewModel.CanAfford(item))
            {
                ViewModel.Buy(item);
                return;
            }

            Manager.Push(new ConfirmPurchase(item, () => ViewModel.Buy(item)));
        }
    }
}
