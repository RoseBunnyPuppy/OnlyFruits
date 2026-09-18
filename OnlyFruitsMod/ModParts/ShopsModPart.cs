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
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Quests;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

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

        #if !DisableDevHelpers

        private List<string> ExtractItemPriceAndStockIds(Dictionary<ISalable, ItemStockInformation> itemPriceAndStock)
        {
            return itemPriceAndStock
                .Select(kvp =>
                {
                    var key1 = kvp.Value.SyncedKey;
                    var key2 = kvp.Value.ItemToSyncStack?.QualifiedItemId;
                    return string.Join(", ", new[] {
                        key1,
                        key2 ?? "",
                    });
                }).ToList();
        }
        #endif
        static HashSet<string> UnmodifiedShops { get; } = new HashSet<string>
        {
            // catalogues allow all shit to be purchased for free
            "Catalogue",
            "Furniture Catalogue",
            "JojaFurnitureCatalogue",
            "JunimoFurnitureCatalogue",
            "RetroFurnitureCatalogue",
            "TrashFurnitureCatalogue",
            "WizardFurnitureCatalogue",

            // nothing here should cost gold
            "DesertTrade",
            // nothing here should cost gold
            "BooksellerTrade",

            // desert festival villager shops
            "DesertFestival_Abigail",
            "DesertFestival_Alex",
            "DesertFestival_Caroline",
            "DesertFestival_Clint",
            "DesertFestival_Demetrius",
            "DesertFestival_Elliott",
            "DesertFestival_Emily",
            "DesertFestival_Evelyn",
            "DesertFestival_George",
            "DesertFestival_Gus",
            "DesertFestival_Haley",
            "DesertFestival_Harvey",
            "DesertFestival_Jas",
            "DesertFestival_Jodi",
            "DesertFestival_Kent",
            "DesertFestival_Leah",
            "DesertFestival_Leo",
            "DesertFestival_Marnie",
            "DesertFestival_Maru",
            "DesertFestival_Pam",
            "DesertFestival_Penny",
            "DesertFestival_Pierre",
            "DesertFestival_Robin",
            "DesertFestival_Sam",
            "DesertFestival_Sebastian",
            "DesertFestival_Shane",
            "DesertFestival_Vincent",

            // calico egg merchant
            "DesertFestival_EggShop",
            // other non-gold shops
            "QiGemShop",
            "IslandTrade",
            "Raccoon",
        };

        private bool TryGetItemIdWithData(
            ISalable key,
            ItemStockInformation value,
            [NotNullWhen(returnValue: true)] out string? itemId,
            [NotNullWhen(returnValue: true)] out ParsedItemData? item
        )
        {
            item = ItemRegistry.GetData(value.SyncedKey);
            if (item != null)
            {
                itemId = value.SyncedKey;
                return true;
            }

            if (value.ItemToSyncStack != null)
            {
                item = ItemRegistry.GetData(value.ItemToSyncStack.QualifiedItemId);
                if (item != null)
                {
                    itemId = value.ItemToSyncStack.QualifiedItemId;
                    return true;
                }
            }

            item = ItemRegistry.GetData(key.QualifiedItemId);
            if (item != null)
            {
                itemId = key.QualifiedItemId;
                return true;
            }
            itemId = default;
            return false;
        }

        
        static Dictionary<string, int> CategoryMultipliers = new()
        {
            ["(F)"] = 1,
        };
        private int GetCategoryMultiplier(string category)
        {
            const int FallbackCategoryMultiplier = 2;
            if (CategoryMultipliers.TryGetValue(category, out var multiplier)) return multiplier;
            return FallbackCategoryMultiplier;
        }
        //static Dictionary<string, HashSet<string>> 
        private void Display_MenuChanged(object? sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is not ShopMenu shopMenu) return;
#if !DisableDevHelpers
            var pairs01 = ExtractItemPriceAndStockIds(shopMenu.itemPriceAndStock);
#endif
            if (UnmodifiedShops.Contains(shopMenu.ShopId)) return;
          
            foreach (var kvp in shopMenu.itemPriceAndStock)
            {
           
                // do nothing if no item id
                if (string.IsNullOrEmpty(kvp.Value.SyncedKey)) continue;
                if (shopMenu.ShopId == "VolcanoShop")
                {
                    if (kvp.Value.SyncedKey == "(O)Book_Diamonds") continue;
                    else if (kvp.Value.SyncedKey == "(B)853") continue;
                }
                else if (shopMenu.ShopId == "LostItems")
                {
                    // apparently _ALL_ items here are 10k
                    // https://stardewvalleywiki.com/Secret_Woods#Lost_Items_Shop
                    kvp.Value.Price = 10_000;
                    continue;
                }
             
                // if the price isnt 'automatic' just keep the original price
                if (kvp.Value.Price != -1 && kvp.Value.Price != 0) continue;
                if (!this.TryGetItemIdWithData(kvp.Key, kvp.Value, out var itemId, out var item)) continue;
               
                var scopeId = item.GetItemTypeId();
              
                // if the original price is known, apply the price
                if (this.origPriceCache.TryGetPriceFull(scopeId, item.ItemId, out var origPrice, out var _))
                {
                    var multiplier = this.GetCategoryMultiplier(scopeId);
                    kvp.Value.Price = origPrice * multiplier; ;
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
