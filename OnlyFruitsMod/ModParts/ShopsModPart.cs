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

        public bool PreloadAssets { get; set; } = PreloadConfiguration.Shops;

        public ShopsModPart(
            ModPartContext context
        ) : base(context)
        {
            this.priceCache = PriceCache.GetOrCreateInstance(this.helper);
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
                            Logger.Instance.Log($"NULL Value shop: {kvp.Key}", LogLevel.Error);
                            continue;
                        }

                        var shopItems = kvp.Value.Items?.ToArray();
                        if (shopItems?.Any() == true)
                        {
                            Logger.Instance.Log($"Shop with items: {kvp.Key}", LogLevel.Error);
                            foreach (var shopItem in shopItems)
                            {
                                var data1 = ItemRegistry.GetData(shopItem.Id);
                                if (shopItem.Price != -1) continue;
                                if (!shopItem.Id.Contains('('))
                                {
                                    if (data1 != null)
                                    {
                                        var itemTypeId = data1.GetItemTypeId();
                                        if (priceCache != null)
                                        {

                                            if (priceCache.TryGetPriceFull(itemTypeId, data1.ItemId, out var _price, out var _wasScopeKnown))
                                            {
                                                _ = 23;
                                            }
                                            else
                                            {
                                                _ = 23;
                                            }
                                        }

                                        _ = 23;
                                    }
                                }
                                Logger.Instance.Log($"{shopItem.Id}", LogLevel.Error);
                                shopItem.Price = 100;
                            }
                        }
                        // skip if there are no 'sale tags'
                        if (kvp.Value.SalableItemTags == null) continue;


                        var tags = kvp.Value.SalableItemTags;
                        var origTags = tags.ToArray();
                        tags.Clear();


                        // if the store never allowed buying fruits, dont allow them to buy anything
                        if (!origTags.Contains(HardcodedSalableTags.Fruits)) continue;

                        // otherwise, re-allow them to buy fruits
                        tags.Add(HardcodedSalableTags.Fruits);
                    }
                });
            }
        }

    }
}
