using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace WeaveUI.Samples.ShopInventory
{
    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
    }

    public sealed class Item : ViewModel
    {
        private static readonly Color[] RarityColors =
        {
            new(0.58f, 0.64f, 0.72f),
            new(0.20f, 0.83f, 0.60f),
            new(0.22f, 0.74f, 0.97f),
            new(0.75f, 0.52f, 0.99f),
            new(0.98f, 0.75f, 0.14f),
        };

        private static readonly Color Affordable = new(0.99f, 0.83f, 0.30f);
        private static readonly Color TooExpensive = new(0.98f, 0.44f, 0.52f);

        private readonly string _icon;
        private bool _canAfford = true;

        public Item(string icon, string name, Rarity rarity, int price, int width, int height, string description)
        {
            _icon = icon;
            Name = name;
            Rarity = rarity;
            Price = price;
            Size = new Vector2Int(width, height);
            Description = description;
            var texture = Resources.Load<Texture2D>($"WeaveUISamples/ShopInventory/Icons/{icon}");
            IconBackground = texture != null ? new StyleBackground(texture) : new StyleBackground(StyleKeyword.None);
        }

        [CreateProperty] public string Name { get; }
        [CreateProperty] public string Description { get; }
        [CreateProperty] public Rarity Rarity { get; }
        [CreateProperty] public StyleBackground IconBackground { get; }
        [CreateProperty] public StyleColor RarityColor => RarityColors[(int)Rarity];

        public int Price { get; }
        public int SellPrice => Price / 2;
        public Vector2Int Size { get; }

        [CreateProperty] public string PriceText => Price.ToString("N0");
        [CreateProperty] public string Summary => $"{Rarity} · {Size.x} × {Size.y} slots";

        /// <summary>Set by the shop when the player's gold changes; the price turns red when it's false.</summary>
        public bool CanAfford
        {
            get => _canAfford;
            set
            {
                if (SetProperty(ref _canAfford, value))
                {
                    Notify(nameof(PriceColor));
                }
            }
        }

        [CreateProperty] public StyleColor PriceColor => _canAfford ? Affordable : TooExpensive;

        /// <summary>The vendor never runs out: buying takes a copy.</summary>
        public Item Copy() => new(_icon, Name, Rarity, Price, Size.x, Size.y, Description);
    }
}
