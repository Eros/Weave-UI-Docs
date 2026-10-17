using System.Collections.Generic;
using Unity.Properties;
using UnityEngine.UIElements;

namespace WeaveUI.Samples.ShopInventory
{
    public sealed class ShopViewModel : ViewModel
    {
        /// <summary>Purchases at or above this price ask for confirmation.</summary>
        public const int ConfirmAbove = 400;

        private int _gold = 760;
        private Item _selected;
        private bool _selectedIsStock;
        private string _status = "Drag an item into your bag to buy it, or onto the vendor to sell it. Double-click buys.";

        public ShopViewModel()
        {
            Bag = new OccupancyMap(8, 6);
            Bag.TryPlace(Catalog.Bow.Copy(), 0, 0, 1, 3);
            Bag.TryPlace(Catalog.Shield.Copy(), 1, 0, 2, 2);
            Bag.TryPlace(Catalog.Potion.Copy(), 3, 0, 1, 1);
            Bag.TryPlace(Catalog.Potion.Copy(), 4, 0, 1, 1);
            Bag.TryPlace(Catalog.Scroll.Copy(), 6, 0, 1, 2);
            Bag.TryPlace(Catalog.Ring.Copy(), 3, 1, 1, 1);
            UpdateAffordability();
        }

        /// <summary>Nine items; the vendor grid binds its ItemsSource here.</summary>
        [CreateProperty]
        public List<Item> Stock { get; } = new()
        {
            Catalog.Sword, Catalog.Axe, Catalog.Bow,
            Catalog.Shield, Catalog.Helmet, Catalog.Ring,
            Catalog.Gem, Catalog.Potion, Catalog.Scroll,
        };

        /// <summary>The bag grid uses this map directly (set in ShopScreen), so items keep their real size.</summary>
        public OccupancyMap Bag { get; }

        [CreateProperty]
        public string GoldText => _gold.ToString("N0");

        [CreateProperty]
        public string Status
        {
            get => _status;
            private set => SetProperty(ref _status, value);
        }

        public Item Selected => _selected;
        public bool SelectedIsStock => _selectedIsStock;

        // Details panel

        [CreateProperty] public string SelectedName => _selected?.Name ?? "Nothing selected";
        [CreateProperty] public string SelectedSummary => _selected?.Summary ?? "Click an item in the shop or your bag.";
        [CreateProperty] public string SelectedDescription => _selected?.Description ?? "";
        [CreateProperty] public StyleBackground SelectedIcon => _selected?.IconBackground ?? new StyleBackground(StyleKeyword.None);
        [CreateProperty] public StyleColor SelectedColor => _selected?.RarityColor ?? new StyleColor(StyleKeyword.Null);

        [CreateProperty]
        public string ActionText => _selected == null ? "Buy" : _selectedIsStock ? $"Buy for {_selected.Price:N0} gold" : $"Sell for {_selected.SellPrice:N0} gold";

        /// <summary>The details panel's button binds its enabled state here.</summary>
        [CreateProperty]
        public bool CanAct => _selected != null && (!_selectedIsStock || _selected.CanAfford);

        public void Select(Item item, bool fromStock)
        {
            _selected = item;
            _selectedIsStock = item != null && fromStock;
            foreach (var property in DetailProperties)
            {
                Notify(property);
            }
        }

        public bool CanAfford(Item item) => item.Price <= _gold;

        /// <summary>Buys a copy into the bag: at (x, y) when dropped there, otherwise in the first free spot.</summary>
        public bool Buy(Item stock, int x = -1, int y = -1)
        {
            if (!CanAfford(stock))
            {
                Status = $"You need {stock.Price - _gold:N0} more gold for the {stock.Name}.";
                return false;
            }

            var bought = stock.Copy();
            var placed = x >= 0 ? Bag.TryPlace(bought, x, y, stock.Size.x, stock.Size.y) : Bag.TryAdd(bought, stock.Size.x, stock.Size.y);
            if (!placed)
            {
                Status = $"No room in your bag for the {stock.Name} ({stock.Size.x} × {stock.Size.y}).";
                return false;
            }

            SetGold(_gold - stock.Price);
            Status = $"Bought the {stock.Name} for {stock.Price:N0} gold.";
            return true;
        }

        public void Sell(Item item)
        {
            if (!Bag.Remove(item))
            {
                return;
            }

            SetGold(_gold + item.SellPrice);
            Status = $"Sold the {item.Name} for {item.SellPrice:N0} gold.";
            if (_selected == item)
            {
                Select(null, false);
            }
        }

        public void Move(Item item, int x, int y)
        {
            if (Bag.TryMove(item, x, y))
            {
                Status = $"Moved the {item.Name}.";
            }
        }

        public void ExplainRejected(Item item) =>
            Status = Stock.Contains(item) && !CanAfford(item)
                ? $"You need {item.Price - _gold:N0} more gold for the {item.Name}."
                : $"The {item.Name} doesn't fit there.";

        void SetGold(int gold)
        {
            _gold = gold;
            Notify(nameof(GoldText));
            UpdateAffordability();
            Notify(nameof(CanAct));
        }

        void UpdateAffordability()
        {
            foreach (var item in Stock)
            {
                item.CanAfford = CanAfford(item);
            }
        }

        private static readonly string[] DetailProperties =
        {
            nameof(SelectedName), nameof(SelectedSummary), nameof(SelectedDescription), nameof(SelectedIcon),
            nameof(SelectedColor), nameof(ActionText), nameof(CanAct),
        };
    }

    /// <summary>The vendor's catalogue. Icons are game-icons.net art (CC BY 3.0) in Resources/WeaveUISamples/ShopInventory/Icons.</summary>
    public static class Catalog
    {
        public static readonly Item Sword = new("sword", "Ashen Blade", Rarity.Rare, 240, 1, 3, "A sword forged in a dying star. Light, quick, and hungry.");
        public static readonly Item Axe = new("axe", "Woodsman's Axe", Rarity.Common, 90, 2, 3, "Splits logs, and the occasional argument.");
        public static readonly Item Bow = new("bow", "Willow Longbow", Rarity.Uncommon, 150, 1, 3, "Quiet as falling leaves.");
        public static readonly Item Shield = new("shield", "Kite Shield", Rarity.Uncommon, 180, 2, 2, "Painted with the crest of a forgotten house.");
        public static readonly Item Helmet = new("helmet", "Crown of Thorns", Rarity.Epic, 520, 2, 2, "Heavy is the head. Heavier still, the helmet.");
        public static readonly Item Ring = new("ring", "Ring of Echoes", Rarity.Rare, 310, 1, 1, "Whispers your last words back to you.");
        public static readonly Item Gem = new("gem", "Starlit Sapphire", Rarity.Legendary, 900, 1, 1, "It hums when the moon is full.");
        public static readonly Item Potion = new("potion", "Healing Draught", Rarity.Common, 25, 1, 1, "Restores 50 health. Tastes of cherries and regret.");
        public static readonly Item Scroll = new("scroll", "Scroll of Return", Rarity.Uncommon, 60, 1, 2, "Read aloud to return home.");
    }
}
