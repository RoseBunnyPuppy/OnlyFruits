using OnlyFruitsMod.Extensions;
using OnlyFruitsMod.Features.Logging;
using OnlyFruitsMod.Features.ModConfiguration;
using OnlyFruitsMod.Features.Prices;
using OnlyFruitsMod.Infrastructure;
using OnlyFruitsMod.Models;
using OnlyFruitsMod.ModParts.Core;
using OnlyFruitsMod.ModParts.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Shops;
using StardewValley.Menus;
using StardewValley.Quests;
using System.Diagnostics;

namespace OnlyFruitsMod.ModParts
{
    /// <summary>
    ///   The shops modifier
    /// </summary>
    public class ShopsModPart : ModPartBase
    {
        private readonly PriceCache priceCache;
        private readonly PriceCache origPriceCache;

        public bool PreloadAssets { get; set; } = PreloadConfiguration.Shops;

        public ShopsModPart(
            ModPartContext context
        ) : base(context)
        {
            this.priceCache = PriceCache.GetOrCreateInstance(this.helper);
            this.origPriceCache = PriceCache.GetOrCreateOrigPrices(this.helper);
            this.helper.Events.Display.MenuChanged += Display_MenuChanged;
        }

        private void Display_MenuChanged(object? sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is not ShopMenu shopMenu) return;
            // catalogues allow all shit to be purchased for free
            if (shopMenu.ShopId == "Catalogue") return;
            
            foreach (var kvp in shopMenu.itemPriceAndStock)
            {
                // do nothing if no item id
                if (string.IsNullOrEmpty(kvp.Value.SyncedKey)) continue;

                // if the price isnt 'automatic' just keep the original price
                if (kvp.Value.Price != -1 && kvp.Value.Price != 0) continue;

                var item = ItemRegistry.GetData(kvp.Value.SyncedKey);

                // if we dont have the item within the registry, do nothing
                if (item == null) continue;

                
                var scopeId = item.GetItemTypeId();
                if (item.ItemId == "BambooPole")
                {
                    kvp.Value.Price = 500;
                    continue;
                }
                // if the original price is known, apply the price
                if (this.origPriceCache.TryGetPriceFull(scopeId, item.ItemId, out var origPrice, out var _))
                {
                    if (scopeId == "(F)")
                    {
                        kvp.Value.Price = origPrice;
                        continue;
                    }
                    else
                    {
                        kvp.Value.Price = origPrice * 2;
                    }
                    continue;
                }
            }
        }

        /// <summary>
        ///     When the configuration changes, invalidate the assets.
        /// </summary>
        protected override void OnModConfigChanged(object? sender, EventArgs e)
        {
            this.helper.GameContent.InvalidateCache(HardcodedAssetPaths.DataShops);
        }

        /// <inheritdoc/>
        protected override void LoadNeededAssets()
        {
            // dont pre-load shit unless needed
            if (!this.PreloadAssets) return;

            this.helper.GameContent.LoadAsset(HardcodedAssetPaths.DataShops);
        }


        private bool ShouldPatchShop(string shopId)
        {
            // dont patch if the challenge isnt enabled.
            if (!this.Context.PerSaveChallengeInstance.IsChallengeEnabled) return false;

            // dont patch if we arent patching non-fruity shops
            if (!this.configInstance.Config.PatchNonFruityShopItems) return false;

            return true;
        }

       
        private void PatchPrice(string shopId, ShopData shopData, ShopItemData shopItem)
        {
            var dataFromId = ItemRegistry.GetData(shopItem.Id);
            // if there is already a fixed price, do nothing
            if (shopItem.Price != -1) return;
            // do nothing for 'traded' items
            if (!string.IsNullOrEmpty(shopItem.TradeItemId)) return;
            if (dataFromId != null)
            {
                if (!this.priceCache.TryGetPriceFull(dataFromId, out var origPrice, out var _)) return;
                shopItem.Price = origPrice * 2;
                return;
            }
            var dataFromItemId = ItemRegistry.GetData(shopItem.ItemId);
            if (dataFromItemId != null)
            {
                if (!this.priceCache.TryGetPriceFull(dataFromItemId, out var origPrice, out var _)) return;
                shopItem.Price = origPrice * 2;
                return;
            }
        }
        private void PatchRequestedAsset(string shopId, ShopData? shopData)
        {
            if (shopData == null) return;
            var shopItems = shopData.Items?.ToArray();
            if (shopItems != null)
            {
                foreach (var shopItem in shopItems)
                {
                    this.PatchPrice(shopId, shopData, shopItem);
                }
            }
            // skip if there are no 'sale tags'
            if (shopData.SalableItemTags == null) return;


            var tags = shopData.SalableItemTags;
            var origTags = tags.ToArray();
            tags.Clear();


            // if the store never allowed buying fruits, dont allow them to buy anything
            if (!origTags.Contains(HardcodedSalableTags.Fruits)) return;

            // otherwise, re-allow them to buy fruits
            tags.Add(HardcodedSalableTags.Fruits);
        }
        /// <inheritdoc/>
        protected override void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataShops))
            {
                e.Edit(asset =>
                {
                    foreach (var kvp in asset.AsAutoDictionary(HardcodedAssetPaths.DataShops).Data)
                    {
                        if (!this.ShouldPatchShop(kvp.Key)) continue;

                        if (kvp.Value == null)
                        {
                            continue;
                        }
                        this.PatchRequestedAsset(kvp.Key, kvp.Value);
                    }
                });
            }
        }

    }
}
