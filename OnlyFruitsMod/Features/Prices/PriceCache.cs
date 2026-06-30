using OnlyFruitsMod.Extensions;
using OnlyFruitsMod.Infrastructure;
using StardewModdingAPI;
using StardewValley.GameData.Objects;

namespace OnlyFruitsMod.Features.Prices
{
    public class PriceCache
    {
        private readonly IModHelper helper;

        public PriceCache(
            IModHelper helper
        )
        {
            this.helper = helper;
        }

        public Dictionary<string, Dictionary<string, int>> ScopedCachedPrices { get; set; } = new Dictionary<string, Dictionary<string, int>>();

        public void Clear()
        {
            this.ScopedCachedPrices.Clear();
        }

        public void Clear(string scope)
        {
            this.ScopedCachedPrices.Remove(scope);
        }

        public bool TryGetPrice(string scope, string itemId, out int price) => this.TryGetPriceFull(scope, itemId, out price, out _);

        public bool TryGetPriceFull(string scope, string itemId, out int price, out bool wasScopeKnown)
        {
            price = default;
            wasScopeKnown = false;
            if (!this.ScopedCachedPrices.TryGetValue(scope, out var lookups))
            {
                if (!this.LoadScope(scope)) return false;
                _ = 23;
                return false;
            }
            wasScopeKnown = true;
            return lookups.TryGetValue(itemId, out price);
        }

        /// <summary>
        ///   Load the data asset for a given item type scope.
        /// </summary>
        private bool LoadScope(string scope)
        {
            switch (scope)
            {
                case ItemIdPrefixes.Boots:
                    this.helper.GameContent.LoadAsset(HardcodedAssetPaths.DataBoots);
                    return true;
                case ItemIdPrefixes.Hats:
                    this.helper.GameContent.LoadAsset(HardcodedAssetPaths.DataHats);
                    return true;
                case ItemIdPrefixes.BigCraftables:
                    this.helper.GameContent.LoadAsset(HardcodedAssetPaths.DataBigCraftables);
                    return true;
                case ItemIdPrefixes.Furniture:
                    this.helper.GameContent.LoadAsset(HardcodedAssetPaths.DataFurniture);
                    return true;
                case ItemIdPrefixes.Mannequins:
                    this.helper.GameContent.LoadAsset(HardcodedAssetPaths.DataMannequins);
                    return true;
                case ItemIdPrefixes.Pants:
                    this.helper.GameContent.LoadAsset(HardcodedAssetPaths.DataPants);
                    return true;
                case ItemIdPrefixes.Shirts:
                    this.helper.GameContent.LoadAsset(HardcodedAssetPaths.DataShirts);
                    return true;
                case ItemIdPrefixes.Tools:
                    this.helper.GameContent.LoadAsset(HardcodedAssetPaths.DataTools);
                    return true;
                case ItemIdPrefixes.Flooring:
                case ItemIdPrefixes.Wallpaper:
                    this.helper.GameContent.LoadAsset(HardcodedAssetPaths.DataAdditionalWallpaperFlooring);
                    return true;
                case ItemIdPrefixes.Trinkets:
                    this.helper.GameContent.LoadAsset(HardcodedAssetPaths.DataTrinkets);
                    return true;
                case ItemIdPrefixes.Weapons:
                    this.helper.GameContent.LoadAsset(HardcodedAssetPaths.DataWeapons);
                    return true;
                default:
                    return false;
            }
        }
        public void SetPriceData(string scope, IDictionary<string, int> priceData)
        {
            this.Clear(scope);

            var scoped = this.ScopedCachedPrices[scope] = new Dictionary<string, int>();
            foreach (var kvp in priceData)
            {
                // cache the original price
                scoped[kvp.Key] = kvp.Value;
            }
        }
        public void SetPriceData(string scope, IDictionary<string, ObjectData> assetData)
        {
            this.Clear(scope);

            var scoped = this.ScopedCachedPrices[scope] = new Dictionary<string, int>();
            foreach (var kvp in assetData)
            {
                // cache the original price
                scoped[kvp.Key] = kvp.Value.Price;
            }
        }
    }
}
