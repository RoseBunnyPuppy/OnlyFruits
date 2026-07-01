using Netcode;
using OnlyFruitsMod.Extensions;
using OnlyFruitsMod.Features.Fruits;
using OnlyFruitsMod.Features.ItemIds;
using OnlyFruitsMod.Features.Logging;
using OnlyFruitsMod.Features.ModConfiguration;
using OnlyFruitsMod.Features.Prices;
using OnlyFruitsMod.Features.ReloadHelpers;
using OnlyFruitsMod.Infrastructure;
using OnlyFruitsMod.ModParts.Core;
using OnlyFruitsMod.ModParts.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Pants;
using StardewValley.GameData.Shirts;
using StardewValley.GameData.SpecialOrders;
using StardewValley.GameData.Tools;
using StardewValley.GameData.Weapons;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace OnlyFruitsMod.ModParts
{
    /// <summary>
    ///   The price changing code
    /// </summary>
    public class PriceModPart : ModPartBase
    {
        const string DisabledCondition = "PLAYER_HAS_TRASH_CAN_LEVEL Current 3 3";
        private HashSet<string> TrashCanPartialIds { get; } 
        private ItemIdConfigModel IdConfigModel { get; set; }

        public DynamicItemManager ItemManager { get; private set; }

        private readonly ReloadManager reloadManager = new();

        private readonly PriceCache priceCache;

        public PriceModPart(
            ModPartContext context
        ) : base(context)
        {
            this.priceCache =  new(
                this.helper
            );
            
            // load config definition assets
            this.IdConfigModel = this.helper.ModContent.Load<ItemIdConfigModel>("assets/fruity_item_ids.json");
            this.TrashCanPartialIds = this.helper.ModContent.Load<string[]>("assets/trashcan_tool_ids.json").ToHashSet();

            // 
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
            helper.Events.Content.AssetReady += Content_AssetReady;
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

            this.helper.GameContent.InvalidateCache(HardcodedAssetPaths.DataBigCraftables);
            this.helper.GameContent.InvalidateCache(HardcodedAssetPaths.DataBoots);
            this.helper.GameContent.InvalidateCache(HardcodedAssetPaths.DataFurniture);
            this.helper.GameContent.InvalidateCache(HardcodedAssetPaths.DataHats);
            this.helper.GameContent.InvalidateCache(HardcodedAssetPaths.DataMannequins);
            this.helper.GameContent.InvalidateCache(HardcodedAssetPaths.DataPants);
            this.helper.GameContent.InvalidateCache(HardcodedAssetPaths.DataShirts);
            this.helper.GameContent.InvalidateCache(HardcodedAssetPaths.DataTools);
            this.helper.GameContent.InvalidateCache(HardcodedAssetPaths.DataTrinkets);
            this.helper.GameContent.InvalidateCache(HardcodedAssetPaths.DataAdditionalWallpaperFlooring);
            this.helper.GameContent.InvalidateCache(HardcodedAssetPaths.DataWeapons);
            this.helper.GameContent.InvalidateCache(HardcodedAssetPaths.DataObjects);
            this.PreloadAssets();

        }

        /// <inheritdoc/>
        protected override void LoadNeededAssets()
        {
            this.helper.GameContent.LoadAsset(HardcodedAssetPaths.CookingRecipes);
            this.helper.GameContent.LoadAsset(HardcodedAssetPaths.CraftingRecipes);
            this.PreloadAssets();
            
        }

        private void PreloadAssets()
        {
            this.helper.GameContent.LoadAsset(HardcodedAssetPaths.DataObjects);
        }

        private bool ShouldPatchItem(string scope, string itemId)
        {
            if (scope == ItemIdPrefixes.Objects) return this.ShouldPatchItem(itemId);
            // dont patch if the challenge isnt enabled.
            if (!this.Context.PerSaveChallengeInstance.IsChallengeEnabled) return false;

            // dont patch if we are restoring
            if (this.configInstance.Config.RestoreAllPrices) return false;

            return false;
        }
        private bool ShouldPatchItem(string itemId)
        {
            // dont patch if the challenge isnt enabled.
            if (!this.Context.PerSaveChallengeInstance.IsChallengeEnabled) return false;

            // dont patch if we are restoring
            if (this.configInstance.Config.RestoreAllPrices) return false;

            // if it is a fruit, we arent patching
            if (this.ItemManager.IsFruityId(ItemIdPrefixes.Objects, itemId)) return false;

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

                var itemScope = item.GetItemTypeId();
                // if there is no original price data from the lodaed data/objects data, skip
                if (!this.priceCache.TryGetPriceFull(itemScope, item.ItemId, out var changedPrice, out var wasScopeKnown))
                {
                    if (wasScopeKnown) continue;
                    _ = 23;
                }
                else
                {
                    // set the  'original price' mod data
                    item.TrySetCachedModDataPrice(changedPrice, force: true);
                }
            }
        }

        #region "Asset Patching"
        /// <summary>
        ///     Indicates whether the challenge is enabled and the player
        ///  whats to prevent trashcan upgrades.
        /// </summary>
        private bool PreventTrashcanUpgrades()
        {
            // upgrades are allowed if we arent using the challenge
            if (!this.Context.PerSaveChallengeInstance.IsChallengeEnabled) return false;

            return !this.configInstance.Config.AllowTrashcanUpgrade;
        }

        /// <summary>
        ///   Constructs a dictionary with value 0 for each key.
        /// </summary>
        private Dictionary<string, int> TreatAllAs0<T>(IDictionary<string, T> original) => Create0ValuesForKeys(original.Keys);
        
        /// <summary>
        ///   Constructs a dictionary with value 0 for each key.
        /// </summary>
        private Dictionary<string, int> Create0ValuesForKeys(IEnumerable<string> keys) => keys.ToDictionary(key => key, _ => 0);
        
        private bool TryHandleRecipeAsset(AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.CookingRecipes))
            {
                e.Edit(asset =>
                {
                    var recipeMap = asset.AsAutoDictionary(HardcodedAssetPaths.CookingRecipes).Data;
                    this.ItemManager.CookingRecipes.LoadRecipeMap(recipeMap);
                });
                return true;
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.CraftingRecipes))
            {
                e.Edit(asset =>
                {
                    var recipeMap = asset.AsAutoDictionary(HardcodedAssetPaths.CraftingRecipes).Data;
                    this.ItemManager.CraftingRecipes.LoadRecipeMap(recipeMap);
                });
                return true;
            }
            return false;
        }
        
        private bool TryHandleItemDataAsset(AssetRequestedEventArgs e)
        {
            // wiki says can be sold to limited shops, and can get money via trashcan
            if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataBoots))
            {
                e.Edit(asset =>
                {
                    var scope = ItemIdPrefixes.Boots;
                    var data = asset.AsAutoDictionary(HardcodedAssetPaths.DataBoots).Data;
                    /* 
                     * The wiki says: 2 	Price 	Unused. The actual price is calculated as (added defence × 100) + (added immunity × 100). 
                     * 
                     *      Therefore, they dont have an overloadable price.  So we need to remove the ability to sell them to stores
                     * as well as limiting the ability to have upgraded trashcans.
                     */
                    this.priceCache.SetPriceData(scope, TreatAllAs0(data));
                });
                return true;
            }
            // wiki says not sellable
            else if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataFurniture))
            {
                e.Edit(asset =>
                {
                    var scope = ItemIdPrefixes.Furniture;
                    var data = asset.AsAutoDictionary(HardcodedAssetPaths.DataFurniture).Data;
                    // The wiki says: Furniture cannot be sold through the Shipping Bin or to any merchant/shop, and cannot be gifted to villagers. 
                    this.priceCache.SetPriceData(scope, TreatAllAs0(data));
                });
                return true;
            }
            // wiki says not sellable
            else if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataHats))
            {
                e.Edit(asset =>
                {
                    var scope = ItemIdPrefixes.Hats;
                    var data = asset.AsAutoDictionary(HardcodedAssetPaths.DataHats).Data;
                    // The wiki says: Hats cannot be sold through the Shipping Bin, to the Hat Mouse, or to any store/shop in the game. 
                    this.priceCache.SetPriceData(scope, TreatAllAs0(data));
                });
                return true;
            }
            // wiki says not sellable
            else if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataMannequins))
            {
                e.Edit(asset =>
                {
                    var scope = ItemIdPrefixes.Mannequins;
                    var data = asset.AsAutoDictionary(HardcodedAssetPaths.DataMannequins).Data;
                    // The wiki says: Cannot be sold 
                    this.priceCache.SetPriceData(scope, TreatAllAs0(data));
                });
                return true;
            }
            // wiki says not sellable
            else if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataPants))
            {
                e.Edit(asset =>
                {
                    var scope = ItemIdPrefixes.Pants;
                    var data = asset.AsAutoDictionary(HardcodedAssetPaths.DataPants).Data;
                    // The wiki says: Shirts, Pants, and Hats cannot be sold anywhere in Stardew Valley. Boots/Shoes can be sold to the Adventurer's Guild. 
                    this.priceCache.SetPriceData(scope, TreatAllAs0(data));
                });
                return true;
            }
            // wiki says not sellable
            else if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataShirts))
            {
                e.Edit(asset =>
                {
                    var scope = ItemIdPrefixes.Shirts;
                    var data = asset.AsAutoDictionary(HardcodedAssetPaths.DataShirts).Data;
                    // The wiki says: Shirts, Pants, and Hats cannot be sold anywhere in Stardew Valley. Boots/Shoes can be sold to the Adventurer's Guild. 
                    this.priceCache.SetPriceData(scope, TreatAllAs0(data));
                });
                return true;
            }
            // wiki says not sellable, but some items do require individual patching
            else if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataTools))
            {
                e.Edit(asset =>
                {
                    var scope = ItemIdPrefixes.Tools;
                    var data = asset.AsAutoDictionary(HardcodedAssetPaths.DataTools).Data;

                    // we want to prevent trashcan upgrades when the challend is enabled
                    if (this.PreventTrashcanUpgrades())
                    {
                        this.PatchTrashCanAssets(data);
                    }

                    // it doesnt appear that we can sell tools
                    this.priceCache.SetPriceData(scope, TreatAllAs0(data));
                });
                return true;
            }
            // wiki says can be sold to limited shops, and can get money via trashcan
            else if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataTrinkets))
            {
                e.Edit(asset =>
                {
                    var scope = ItemIdPrefixes.Trinkets;
                    var data = asset.AsAutoDictionary(HardcodedAssetPaths.DataTrinkets).Data;
                    // every trinket seems to be sellable for 1000
                    this.priceCache.SetPriceData(scope, TreatAllAs0(data));
                });
                return true;
            }
            // wiki says not sellable
            else if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataAdditionalWallpaperFlooring))
            {
                e.Edit(asset =>
                {
                    var data = asset.GetData<List<ModWallpaperOrFlooring>>();

                    // unsure
                    this.priceCache.SetPriceData(ItemIdPrefixes.Flooring, Create0ValuesForKeys(data.Where(x => x.IsFlooring).Select(item => item.Id)));
                    this.priceCache.SetPriceData(ItemIdPrefixes.Wallpaper, Create0ValuesForKeys(data.Where(x => !x.IsFlooring).Select(item => item.Id)));
                });
                return true;
            }
            // wiki doesnt say if it is sellable
            else if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataWeapons))
            {
                e.Edit(asset =>
                {
                    var scope = ItemIdPrefixes.Weapons;
                    var data = asset.AsAutoDictionary(HardcodedAssetPaths.DataWeapons).Data;
                    // every trinket seems to be sellable for 1000 (we prevent selling them via shop filters)
                    this.priceCache.SetPriceData(scope, TreatAllAs0(data));
                });
                return true;
            }
            // wiki says sellable
            else if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataBigCraftables))
            {
                e.Edit(asset =>
                {
                    var scope = ItemIdPrefixes.BigCraftables;
                    var data = asset.AsAutoDictionary(HardcodedAssetPaths.DataBigCraftables).Data;

                    //reset the price cache for the scope
                    this.priceCache.SetPriceData(scope, data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Price));

                    // set non-fruits to have a sale price of 0
                    foreach (var (partialItemId, itemData) in data)
                    {
                        itemData.Price = 0;
                    }
                });
                return true;
            }
            // wiki says sellable
            else if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataObjects))
            {
                e.Edit(asset =>
                {

                    var scope = ItemIdPrefixes.Objects;
                    var data = asset.AsAutoDictionary(HardcodedAssetPaths.DataObjects).Data;

                    // reset the price cache
                    this.priceCache.SetPriceData(scope, data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Price));

                    // find items
                    this.ItemManager.ApplyObjectData(data);

                    // set non-fruits to have a sale price of 0
                    foreach (var (partialItemId, itemData) in data)
                    {
                        if (!this.ShouldPatchItem(scope, partialItemId)) continue;
                        itemData.Price = 0;
                    }
                });
                return true;
            }
            return false;
        }

        private void Content_AssetReady(object? sender, AssetReadyEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataObjects))
            {
                Logger.Instance.LogAssetReady(this, e.NameWithoutLocale);
                // re-apply the live data changes if needed
                if (this.reloadManager.ConsumeReload())
                {
                    this.PatchLiveData();
                }
            }
        }

        protected override void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (this.TryHandleRecipeAsset(e)) return;
            else if (this.TryHandleItemDataAsset(e)) return;
        }
        
        /// <summary>
        ///   Prevent a hardcoded list of trashcans from being upgraded
        /// </summary>
        private void PatchTrashCanAssets(IDictionary<string, ToolData> toolDataMap)
        {
            foreach (var (key, toolData) in toolDataMap)
            {
                // skip if not a trash can item
                if (!this.TrashCanPartialIds.Contains(key)) continue;

                // patch the upgrade-from conditions
                foreach (var upgradeFrom in toolData.UpgradeFrom)
                {
                    if (upgradeFrom.Condition.StartsWith("PLAYER_HAS_TRASH_CAN_LEVEL"))
                    {
                        upgradeFrom.Condition = DisabledCondition;
                    }
                }
            }
        }

        #endregion "Asset Patching"

        #region "Live Data Replacing"

        private void GameLoop_SaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            this.PatchLiveData();
        }

        private void PatchLiveData()
        {
            // do nothing if no game is loaded
            if (!Game1.hasLoadedGame) return;

            // do nothing if we dont currently have any per-save data
            if (!this.Context.PerSaveChallengeInstance.HasPerSaveLoaded) return;

            // patch if trashcan upgrades are disabled
            if (this.PreventTrashcanUpgrades())
            {
                this.PatchTrashCanLevel();
            }
            // otherwise, restore the trashcan level
            else
            {
                this.RestoreTrashCanLevel();
            }

            // patch the player's personal inventory
            this.PatchInventoryItems(Game1.player.Items);

            foreach (GameLocation? location in Game1.locations)
            {
                if (location == null) continue;

                // patch the farmhouse
                if (location is FarmHouse farmHouse)
                {
                    this.PatchInventoryItems(farmHouse.fridge.Value.Items);
                }


                // patch the placed chests
                foreach (var pairKvp in location.objects.Pairs)
                {
                    if (pairKvp.Value is Chest chest)
                    {
                        this.PatchInventoryItems(chest.Items);
                    }
                }
            }
        }

        private void RestoreTrashCanLevel()
        {
            if (!Game1.player.modData.TryGetValue(HardcodedModDataKeys.OriginalTrashCanDataKey, out var currentTrashCanLevel)) return;
            Game1.player.trashCanLevel = int.Parse(currentTrashCanLevel);
        }
        private void PatchTrashCanLevel()
        {
            if (!Game1.player.modData.TryGetValue(HardcodedModDataKeys.OriginalTrashCanDataKey, out _))
            {
                Game1.player.modData[HardcodedModDataKeys.OriginalTrashCanDataKey] = Game1.player.trashCanLevel.ToString();
            }
            Game1.player.trashCanLevel = 0;
        }


        /// <summary>
        ///   Store the current non-zero price to the item's moddata cache.
        /// </summary>
        private void PatchPrice(Item item, NetInt priceField)
        {
            // if there is no price set, do nothing
            if (priceField.Value == 0) return;

            // set the directly cached item price (if not already set)
            item.TrySetCachedModDataPrice(priceField.Value);

            // clear the price
            priceField.Value = 0;
        }

        /// <summary>
        ///     Attempt to restore the price data for an item.
        ///   Returns whether the item had or now has a price.
        /// </summary>
        private bool RestorePrice(Item item, NetInt priceField)
        {
            // if there is alread a price set, do nothing
            if (priceField.Value != 0) return true;

            // if there is "original price data" directly stored on the item, use it
            if (item.TryGetDirectlyCachedModDataPrice(out var directPrice))
            {
                priceField.Value = directPrice;
                return true;
            }

            // if there is a cached price, use it
            if (this.priceCache.TryGetPriceFull(item, out var assetPrice, out var wasScopeKnown))
            {
                // dont update anything if the price is the same
                if (priceField.Value == assetPrice) return true;
                
                // otherwise, use the cached price
                priceField.Value = assetPrice;
                return true;
            }

            return false;
        }

        /// <summary>
        ///   Apply needed patches to an item.
        /// </summary>
        private void PatchInventoryItem(Item item)
        {
            // find the 'item price' field
            var priceField = item.NetFields.TryGetFieldByName<NetInt>(HardcodedNetFieldNames.ItemPrice);

            // do nothing if there is no 'item price' field
            if (priceField == null) return;

            // if this is a patchable item, patch it
            if (this.ShouldPatchItem(item.GetItemTypeId(), item.ItemId))
            {
                this.PatchPrice(item, priceField);
                return;
            }
            // otherwise, restore the price data if needed
            else
            {
                this.RestorePrice(item, priceField);
            }
        }

        /// <summary>
        ///   Apply needed patches to every item in the inventory.
        /// </summary>
        private void PatchInventoryItems(StardewValley.Inventories.Inventory inventory)
        {
            foreach (var item in inventory)
            {
                if (item == null) continue;
                this.PatchInventoryItem(item);
            }
        }

        #endregion "Live Data Replacing"

    }
}
