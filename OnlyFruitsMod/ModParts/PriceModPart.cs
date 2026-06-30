using Netcode;
using OnlyFruitsMod.Extensions;
using OnlyFruitsMod.Features.Fruits;
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
        private ItemIdConfigModel IdConfigModel { get; set; } = DefaultItemIdConfigProvider.GetDefaults();

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
        private Dictionary<string, int> TreatAllAs0<T>(IDictionary<string, T> original) => original.ToDictionary(kvp => kvp.Key, kvp => 0);
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
            else if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataBoots))
            {
                e.Edit(asset =>
                {
                    var scope = ItemIdPrefixes.Boots;
                    var data = asset.AsAutoDictionary(HardcodedAssetPaths.DataBoots).Data;
                    // 2 	Price 	Unused. The actual price is calculated as (added defence × 100) + (added immunity × 100). 
                    _ = 23;
                    this.priceCache.SetPriceData(scope, TreatAllAs0(data));
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataFurniture))
            {
                e.Edit(asset =>
                {
                    var scope = ItemIdPrefixes.Furniture;
                    var data = asset.AsAutoDictionary(HardcodedAssetPaths.DataFurniture).Data;
                    // The wiki says: Furniture cannot be sold through the Shipping Bin or to any merchant/shop, and cannot be gifted to villagers. 
                    this.priceCache.SetPriceData(scope, TreatAllAs0(data));
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataHats))
            {
                e.Edit(asset =>
                {
                    var scope = ItemIdPrefixes.Hats;
                    var data = asset.AsAutoDictionary(HardcodedAssetPaths.DataHats).Data;
                    // The wiki says: Hats cannot be sold through the Shipping Bin, to the Hat Mouse, or to any store/shop in the game. 
                    this.priceCache.SetPriceData(scope, TreatAllAs0(data));
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataMannequins))
            {
                e.Edit(asset =>
                {
                    var scope = ItemIdPrefixes.Mannequins;
                    var data = asset.AsAutoDictionary(HardcodedAssetPaths.DataMannequins).Data;
                    // The wiki says: Cannot be sold 
                    this.priceCache.SetPriceData(scope, TreatAllAs0(data));
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataPants))
            {
                e.Edit(asset =>
                {
                    var scope = ItemIdPrefixes.Pants;
                    var data = asset.AsAutoDictionary(HardcodedAssetPaths.DataPants).Data;
                    // The wiki says: Shirts, Pants, and Hats cannot be sold anywhere in Stardew Valley. Boots/Shoes can be sold to the Adventurer's Guild. 
                    this.priceCache.SetPriceData(scope, TreatAllAs0(data));
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataShirts))
            {
                e.Edit(asset =>
                {
                    var scope = ItemIdPrefixes.Shirts;
                    var data = asset.AsAutoDictionary(HardcodedAssetPaths.DataShirts).Data;
                    // The wiki says: Shirts, Pants, and Hats cannot be sold anywhere in Stardew Valley. Boots/Shoes can be sold to the Adventurer's Guild. 
                    this.priceCache.SetPriceData(scope, TreatAllAs0(data));
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataTools))
            {
                e.Edit(asset =>
                {
                    var scope = ItemIdPrefixes.Tools;
                    var data = asset.AsAutoDictionary(HardcodedAssetPaths.DataTools).Data;

                    // we want to prevent trashcan upgrades when the challend is enabled
                    if (this.PreventTrashcanUpgrades())
                    {
                        const string DisabledCondition = "PLAYER_HAS_TRASH_CAN_LEVEL Current 3 3";
                        data["CopperTrashCan"].UpgradeFrom[0].Condition = DisabledCondition;
                        data["SteelTrashCan"].UpgradeFrom[0].Condition = DisabledCondition;
                        data["GoldTrashCan"].UpgradeFrom[0].Condition = DisabledCondition;
                        data["IridiumTrashCan"].UpgradeFrom[0].Condition = DisabledCondition;
                    }
                    
                    // it doesnt appear that we can sell tools
                    this.priceCache.SetPriceData(scope, TreatAllAs0(data));
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataTrinkets))
            {
                e.Edit(asset =>
                {
                    var scope = ItemIdPrefixes.Trinkets;
                    var data = asset.AsAutoDictionary(HardcodedAssetPaths.DataTrinkets).Data;
                    // every trinket seems to be sellable for 1000
                    this.priceCache.SetPriceData(scope, TreatAllAs0(data));
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataAdditionalWallpaperFlooring))
            {
                e.Edit(asset =>
                {
                    var data = asset.GetData<List<ModWallpaperOrFlooring>>();
                    
                    // unsure
                    this.priceCache.SetPriceData(ItemIdPrefixes.Flooring, data.Where(x => x.IsFlooring).ToDictionary(item => item.Id, item => 0));
                    this.priceCache.SetPriceData(ItemIdPrefixes.Wallpaper, data.Where(x => !x.IsFlooring).ToDictionary(item => item.Id, item => 0));
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataWeapons))
            {
                e.Edit(asset =>
                {
                    var scope = ItemIdPrefixes.Weapons;
                    var data = asset.AsAutoDictionary(HardcodedAssetPaths.DataWeapons).Data;
                    // every trinket seems to be sellable for 1000 (we prevent selling them via shop filters)
                    this.priceCache.SetPriceData(scope, TreatAllAs0(data));
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataBigCraftables))
            {
                e.Edit(asset =>
                {
                    var scope = ItemIdPrefixes.BigCraftables;
                    var data = asset.AsAutoDictionary(HardcodedAssetPaths.DataBigCraftables).Data;
                    
                    //reset the price cache for the scope
                    this.priceCache.SetPriceData(scope, data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Price));

                    // set non-fruits to have a sale price of 0
                    foreach (var (itemID, itemData) in data)
                    {
                        itemData.Price = 0;
                    }
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataObjects))
            {
                e.Edit(asset =>
                {
                    
                    var scope = ItemIdPrefixes.Objects;
                    var data = asset.AsAutoDictionary(HardcodedAssetPaths.DataObjects).Data;
                    
                    // reset the price cache
                    this.priceCache.SetPriceData(scope, data);

                    // find items
                    this.ItemManager.ApplyAssetData(data);

                    // set non-fruits to have a sale price of 0
                    foreach ((string itemID, ObjectData itemData) in data)
                    {
                        if (!this.ShouldPatchItem(scope, itemID)) continue;
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


        #endregion "Asset Patching"

        #region "Live Data Replacing"

        private void GameLoop_SaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            this.OverrideExistingItemPrices();
        }

        private void OverrideExistingItemPrices()
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

        private void RestoreTrashCanLevel()
        {
            if (!Game1.player.modData.TryGetValue(HardcodedModDataKeys.OriginalTrashCanDataKey, out var currentTrashCanLevel)) return;
            Game1.player.trashCanLevel = int.Parse(currentTrashCanLevel);
        }
        private void PatchTrashCanLevel()
        {
            if (!Game1.player.modData.TryGetValue(HardcodedModDataKeys.OriginalTrashCanDataKey, out var currentTrashCanLevel))
            {
                Game1.player.modData[HardcodedModDataKeys.OriginalTrashCanDataKey] = Game1.player.trashCanLevel.ToString();
            }
            Game1.player.trashCanLevel = 0;
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
            var itemScope = item.GetItemTypeId();
            if (itemScope == ItemIdPrefixes.BigCraftables)
                _ = 23;
            if (this.priceCache.TryGetPriceFull(itemScope, item.ItemId, out var assetPrice, out var wasScopeKnown))
            {
                // dont update anything if the price is the same
                if (priceField.Value == assetPrice) return true;
                priceField.Value = assetPrice;
                return true;
            }
            else if (wasScopeKnown) return false;
            _ = 23;

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
            if (this.ShouldPatchItem(item.GetItemTypeId(), item.ItemId))
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
