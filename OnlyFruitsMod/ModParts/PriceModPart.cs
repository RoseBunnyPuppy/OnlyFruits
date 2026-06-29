using OnlyFruitsMod.Extensions;
using OnlyFruitsMod.Features.Fruits;
using OnlyFruitsMod.Features.ModConfiguration;
using OnlyFruitsMod.Features.Prices;
using OnlyFruitsMod.Features.ReloadHelpers;
using OnlyFruitsMod.Infrastructure;
using OnlyFruitsMod.Models;
using OnlyFruitsMod.ModParts.Core;
using OnlyFruitsMod.ModParts.Models;
using Netcode;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Objects;
using StardewValley.GameData.SpecialOrders;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyFruitsMod.ModParts
{

    
    /// <summary>
    ///   The price changing code
    /// </summary>
    public class PriceModPart : ModPartBase
    {
        private ItemIdConfigModel IdConfigModel { get; set; } = DefaultItemIdConfigProvider.GetDefaults();
        public bool Verbose { get; set; } = false;

        public DynamicItemManager ItemManager { get; private set; }

        private readonly ReloadManager reloadManager = new();

        private readonly PriceCache priceCache = new();

        public PriceModPart(
            ModPartContext context
        ) : base(context)
        {
            this.ItemManager = new DynamicItemManager(
                this.IdConfigModel,
                this.monitor,
                this.configInstance
            );
        }

        /// <inheritdoc/>
        protected override void AttachListeners()
        {
            base.AttachListeners();
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.Player.InventoryChanged += Player_InventoryChanged;
        }

        /// <summary>
        ///     When the configuration changes, invalidate the assets
        ///   and re-initialize the live data once the assets have 
        ///   reloaded.
        /// </summary>
        protected override void OnModConfigChanged(object? sender, EventArgs e)
        {
            this.reloadManager.EnqueueReload();
            this.helper.GameContent.InvalidateCache(HardcodedAssetPaths.DataObjects);
        }

        /// <inheritdoc/>
        protected override void LoadNeededAssets()
        {
            this.helper.GameContent.LoadAsset(HardcodedAssetPaths.CookingRecipes);
            this.helper.GameContent.LoadAsset(HardcodedAssetPaths.CraftingRecipes);
        }

        private bool ShouldPatchItem(string itemId)
        {
            // dont patch if we are restoring
            if (this.configInstance.Config.RestoreAllPrices) return false;

            // if it is a fruit, we arent patching
            if (this.ItemManager.IsFruityId(itemId)) return false;

            // otherwise, we patch
            return true;
        }

        private void Player_InventoryChanged(object? sender, InventoryChangedEventArgs e)
        {
            if (!e.Added.Any()) return;

            foreach (var item in e.Added)
            {
                // skip null items
                if (item == null) continue;
                
                // do nothing if the stack already has 'original price' mod data
                if (item.TryGetDirectlyCachedModDataPrice(out _)) continue;

                // if there is no original price data from the lodaed data/objects data, skip
                if (!this.priceCache.CachedPrices.TryGetValue(item.ItemId, out var changedPrice)) continue;

                // set the  'original price' mod data
                item.TrySetCachedModDataPrice(changedPrice, force: true);
            }
        }

        #region "Asset Patching"
        protected override void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.CookingRecipes.Name))
            {
                this.ItemManager.CookingRecipes.LoadFromAssetEvent(e);
                return;
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.CraftingRecipes))
            {
                this.ItemManager.CraftingRecipes.LoadFromAssetEvent(e);
                return;
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataObjects))
            {
               
                e.Edit(asset =>
                {
                    var data = asset.AsAutoDictionary(HardcodedAssetPaths.DataObjects).Data;
                    
                    // reset the price cache
                    this.priceCache.ResetPriceData(data);

                    // find items
                    this.ItemManager.ApplyAssetData(data);

                    // set non-fruits to have a sale price of 0
                    foreach ((string itemID, ObjectData itemData) in data)
                    {
                        if (!this.ShouldPatchItem(itemID)) continue;

                        if (this.Verbose) this.monitor.Log($"Preventing sale of asset: {e.Name} ({itemID}): {JsonConvert.SerializeObject(e, Formatting.Indented)}.", LogLevel.Debug);
                        itemData.Price = 0;
                    }
                });


                // re-apply the live data changes if needed
                if (this.reloadManager.ConsumeReload())
                {
                    this.OverrideExistingItemPrices();
                }
            }
        }


        private void Summarize(IDictionary<string, ObjectData> data, DynamicItemManager manager)
        {

            var summary = manager.FullItemIds.Select(pair => new
            {
                itemId = pair.Id,
                pair.Reason,
                data[pair.Id].Name,
                data[pair.Id].DisplayName,
            })
            .OrderBy(x => x.Reason)
            .ThenBy(x => x.Name)
            .ToArray();

            var orderedSummary = summary.Select((x, idx) => new { item = x, index = idx }).GroupBy(x => x.item.Reason).Select(grp => new
            {
                Reason = grp.Key,
                Items = grp.OrderBy(x => x.index).Select(x => x.item).ToArray(),
            }).OrderBy(pair => pair.Reason).ToArray();


            foreach (var group in orderedSummary)
            {
                if (this.Verbose) this.monitor.Log($"==== {group.Reason} ====", LogLevel.Debug);
                foreach (var item in group.Items)
                {
                    if (this.Verbose) this.monitor.Log($"{item.Name}", LogLevel.Debug);
                }
                if (this.Verbose) this.monitor.Log("", LogLevel.Debug);
            }
        }


        #endregion "Asset Patching"

        #region "Live Data Replacing"

        private void GameLoop_SaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            this.OverrideExistingItemPrices();
        }

        private void OverrideExistingItemPrices()
        {
            if (!Game1.hasLoadedGame) return;
            this.UpdateInventoryPrices(Game1.player.Items);
            foreach (GameLocation? location in Game1.locations)
            {
                if (location == null) continue;

                if (location is FarmHouse farmHouse)
                {
                    this.UpdateInventoryPrices(farmHouse.fridge.Value.Items);
                }


                foreach (var kvp in location.objects.Pairs)
                {
                    if (kvp.Value is Chest chest)
                    {
                        this.UpdateInventoryPrices(chest.Items);
                    }
                }
            }
        }

        /// <summary>
        ///   Store a cached 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="priceField"></param>
        private void PatchPrice(Item item, NetInt priceField)
        {
            // set the directly cached item price (if not already set)
            item.TrySetCachedModDataPrice(priceField.Value);

            // clear the price
            priceField.Value = 0;
        }
        private bool RestorePrice(Item item, NetInt priceField)
        {
            // if there is a directly stored cached price, apply it
            if (item.TryGetDirectlyCachedModDataPrice(out var directPrice))
            {
                priceField.Value = directPrice;
                return true;
            }

            // otherwise, try to apply the asset price
            if (this.priceCache.CachedPrices.TryGetValue(item.ItemId, out var assetPrice))
            {
                priceField.Value = assetPrice;
                return true;
            }

            return false;
        }

        private void UpdateItemPrice(Item item)
        {
            // find the 'item price' field
            var priceField = item.NetFields.TryGetFieldByName<NetInt>(HardcodedNetFieldNames.ItemPrice);

            // do nothing if there is no 'item price' field
            if (priceField == null) return;

            // get the current price
            var curValue = priceField.Value;

            // if this is a patchable item, patch it
            if (this.ShouldPatchItem(item.ItemId))
            {
                // if there is no price set, do nothing
                if (curValue == 0) return;

                this.PatchPrice(item, priceField);
                return;
            }
            else
            {
                // if there is a price set, do nothing
                if (curValue != 0) return;
                this.RestorePrice(item, priceField);
            }
        }

        /// <summary>
        ///   Update the prices for all items in the inventory.
        /// </summary>
        private void UpdateInventoryPrices(StardewValley.Inventories.Inventory inventory)
        {
            foreach (var item in inventory)
            {
                if (item == null) continue;
                this.UpdateItemPrice(item);
            }
        }

        #endregion "Live Data Replacing"

    }
}
