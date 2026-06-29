using OnlyFruitsMod.Extensions;
using OnlyFruitsMod.Features.ModConfiguration;
using OnlyFruitsMod.Infrastructure;
using OnlyFruitsMod.Models;
using OnlyFruitsMod.ModParts.Core;
using OnlyFruitsMod.ModParts.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Extensions;
using StardewValley.GameData.Shops;

namespace OnlyFruitsMod.ModParts
{
    /// <summary>
    ///   The shops modifier
    /// </summary>
    public class ShopsModPart : ModPartBase
    {
        public bool PreloadAssets { get; set; } = PreloadConfiguration.Shops;

        public ShopsModPart(
            ModPartContext context
        ) : base(context)
        {

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
            // dont patch if restoring item prices
            if (this.configInstance.Config.RestoreAllPrices) return false;
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
