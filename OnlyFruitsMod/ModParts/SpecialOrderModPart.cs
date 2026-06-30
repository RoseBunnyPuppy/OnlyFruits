using Force.DeepCloner;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using OnlyFruitsMod.Extensions;
using OnlyFruitsMod.Features.ModConfiguration;
using OnlyFruitsMod.Features.Quests;
using OnlyFruitsMod.Features.Quests.Models;
using OnlyFruitsMod.Features.ReloadHelpers;
using OnlyFruitsMod.Infrastructure;
using OnlyFruitsMod.Models;
using OnlyFruitsMod.ModParts.Core;
using OnlyFruitsMod.ModParts.Models;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.SpecialOrders;
using StardewValley.Quests;
using StardewValley.SpecialOrders;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace OnlyFruitsMod.ModParts
{
    /// <summary>
    ///   The special-order modification code.
    /// </summary>
    public class SpecialOrderModPart : ModPartBase
    {
        public bool PreloadAssets { get; set; } = PreloadConfiguration.SpecialOrders;
        
        private readonly ReloadManager reloadManager = new();
        private readonly SpecialOrderStatusDeterminer questPatchStatusHelper;

        public SpecialOrderKeysConfigModel SpecialOrderKeysConfigModel { get; } = DefaultSpecialOrderKeysProvider.Create();

        /// <summary>
        ///   The cached version of the SpecialOrderStrings.
        /// </summary>
        private Dictionary<string, string> CachedSpecialOrderStrings { get; set; } = new Dictionary<string, string>();

        private readonly LewisCropRemapper lewisCropRemapper = new();
        private record SpecialOrderReward(SpecialOrderData SpecialOrder, SpecialOrderRewardData RewardData);

        static Regex RexTextMatch = new Regex("\\[([^\\]]+)\\]", RegexOptions.Compiled);

        public SpecialOrderModPart(
            ModPartContext context
        ) : base(context)
        {
            this.questPatchStatusHelper = new SpecialOrderStatusDeterminer(
                configInstance, 
                this.SpecialOrderKeysConfigModel,
                this.Context.PerSaveChallengeInstance
            );
        }

        /// <inheritdoc/>
        protected override void AttachListeners()
        {
            base.AttachListeners();
            helper.Events.GameLoop.SaveLoaded += this.GameLoop_SaveLoaded;
        }

        /// <inheritdoc/>
        protected override void LoadNeededAssets()
        {
            this.helper.GameContent.LoadAsset(HardcodedAssetPaths.SpecialOrderStrings);
            this.DoPreloadIfNeeded();
        }

        private void DoPreloadIfNeeded()
        {
            // dont pre-load shit unless needed
            if (!this.PreloadAssets) return;
            this.helper.GameContent.LoadAsset(HardcodedAssetPaths.DataSpecialOrders);
        }

        /// <summary>
        ///     When the configuration changes, invalidate the assets
        ///   and re-initialize the live data once the assets have 
        ///   reloaded.
        /// </summary>
        protected override void OnModConfigChanged(object? sender, EventArgs e)
        {
            this.reloadManager.EnqueueReload();
            this.helper.GameContent.InvalidateCache(HardcodedAssetPaths.DataSpecialOrders);
            this.DoPreloadIfNeeded();
        }



        #region "Asset Patching"

        private void PatchAsset(string questId, SpecialOrderData specialOrderData, OrderPatchingFlavors flavor)
        {
            switch (flavor)
            {
                case OrderPatchingFlavors.NonFruity:
                    QuestRewardHelper.Instance.SetMoneyRewardToZero(questId, specialOrderData);
                    return;
                case OrderPatchingFlavors.NonFruityQi:
                case OrderPatchingFlavors.PotentiallyNonFruityQi:
                    QuestRewardHelper.Instance.SetGemRewardToZero(specialOrderData);
                    QuestRewardHelper.Instance.SetMoneyRewardToZero(questId, specialOrderData);
                    return;
                case OrderPatchingFlavors.LewisSpecialOrder:
                    this.ModifyLewisSpecialOrderAssets(questId, specialOrderData);
                    return;
                case OrderPatchingFlavors.CarolineSpecialOrder:
                    this.ModifyCarolineSpecialOrderAssets(questId, specialOrderData);
                    return;
                case OrderPatchingFlavors.DontPatch: 
                    return;
                default:
                    this.monitor.Log($"Unsuppored asset patch status '{flavor}'.  Treating as 'do not patch'", LogLevel.Error);
                    return;
            }
        }
        protected override void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataSpecialOrders))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsAutoDictionary(HardcodedAssetPaths.DataSpecialOrders).Data;
                    foreach (var (questId, specialOperationData) in data)
                    {
                        var flavor = this.questPatchStatusHelper.GetPatchingFlavor(questId);
                        this.PatchAsset(questId, specialOperationData, flavor);
                    }
                    // re-apply the live data changes if needed
                    if (this.reloadManager.ConsumeReload())
                    {
                        this.OverrideQuestRewards();
                    }
                });
               
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.SpecialOrderStrings))
            {
                e.Edit(asset =>
                {
                    this.CachedSpecialOrderStrings = new Dictionary<string, string>(asset.AsAutoDictionary(HardcodedAssetPaths.SpecialOrderStrings).Data);
                });
            }
        }

        private void ModifyCarolineSpecialOrderAssets(string source, SpecialOrderData? specialOrderData)
        {
            if (specialOrderData == null) return;

            var randomizedCrops = specialOrderData.RandomizedElements.GetByName(HardcodedQuestConstants.RandomizedElementNames.Crop);
            if (randomizedCrops == null) return;
            var element = randomizedCrops.Values.Single();
            element.Value = PickItemHelper.Instance.Serialize(new string[] {
                "Pineapple",
            });


        }
        private void ModifyLewisSpecialOrderAssets(string source, SpecialOrderData? specialOrderData)
        {
            if (specialOrderData == null) return;

            var randomizedCrops = specialOrderData.RandomizedElements.GetByName(HardcodedQuestConstants.RandomizedElementNames.Crop);
            if (randomizedCrops == null) return;

            // option 1: remove 'season_spring' because no fruits in it
            // option 2: Replace with  "PICK_ITEM Strawberry"
            var springElement = randomizedCrops.Values.ExpectSingleByRequiredTag("season_spring");
            springElement.Value = PickItemHelper.Instance.Serialize(new string[] {
                "Strawberry",
            });

            // (season_summer) Replace with  "PICK_ITEM Blueberry, Melon, Hot Pepper"
            var summerElement = randomizedCrops.Values.ExpectSingleByRequiredTag("season_summer");
            summerElement.Value = PickItemHelper.Instance.Serialize(new string[] {
                "Blueberry",
                "Melon",
                "Hot Pepper",
            });

            // (season_fall) Replace with "PICK_ITEM Cranberries, Grape"
            var fallElement = randomizedCrops.Values.ExpectSingleByRequiredTag("season_fall");
            fallElement.Value = PickItemHelper.Instance.Serialize(new string[] {
                "Cranberries",
                "Grape",
            });

        }


        #endregion "Asset Patching"


        #region "Live Data Replacing"
        private void ModifyLewisSpecialOrder(SpecialOrder specialOrder)
        {

            var selectedCrop = specialOrder.preSelectedItems[HardcodedQuestConstants.RandomizedElementNames.Crop];
            if (selectedCrop == null) throw new InvalidOperationException();
            var updatedCropKey = lewisCropRemapper.GetFruitCrop(selectedCrop, specialOrder.generationSeed.Value);
            if (updatedCropKey == selectedCrop) return;
            specialOrder.preSelectedItems[HardcodedQuestConstants.RandomizedElementNames.Crop] = updatedCropKey;
        }
        private void ModifyCarolineSpecialOrder(SpecialOrder specialOrder)
        {
            const string PineappleCropId = "(O)832";
            var selectedCrop = specialOrder.preSelectedItems[HardcodedQuestConstants.RandomizedElementNames.Crop];
            if (selectedCrop == null) throw new InvalidOperationException();
            if (selectedCrop == PineappleCropId) return;
            specialOrder.preSelectedItems[HardcodedQuestConstants.RandomizedElementNames.Crop] = PineappleCropId;
        }

        private void ModifyLiveSpecialOrder(string questId, SpecialOrder specialOrder, OrderPatchingFlavors flavor)
        {
            switch (flavor)
            {
                case OrderPatchingFlavors.DontPatch:
                    QuestRewardHelper.Instance.RestoreMoneyReward(specialOrder);
                    QuestRewardHelper.Instance.RestoreGemReward(specialOrder);
                    return;
                case OrderPatchingFlavors.NonFruity:
                    QuestRewardHelper.Instance.SetMoneyRewardToZero(specialOrder);
                    return;
                case OrderPatchingFlavors.NonFruityQi:
                case OrderPatchingFlavors.PotentiallyNonFruityQi:
                    QuestRewardHelper.Instance.SetMoneyRewardToZero(specialOrder);
                    QuestRewardHelper.Instance.SetGemRewardToZero(specialOrder);
                    return;
                case OrderPatchingFlavors.LewisSpecialOrder:
                    this.ModifyLewisSpecialOrder(specialOrder);
                    return;
                case OrderPatchingFlavors.CarolineSpecialOrder:
                    this.ModifyCarolineSpecialOrder(specialOrder);
                    return;
                default:
                    this.monitor.Log($"Unsuppored live patch status '{flavor}'.  Treating as 'do not patch'", LogLevel.Error);
                    return;
            }
        }


        private void OverrideQuestRewards()
        {
            // abort if we arent in a game
            if (!Game1.hasLoadedGame) return;

            // do nothing if we dont currently have any per-save data
            if (!this.Context.PerSaveChallengeInstance.HasPerSaveLoaded) return;

            var specialOrders = Game1.player.team.specialOrders;

            // modify existing special orders
            foreach (var specialOrder in specialOrders)
            {
                if (specialOrder == null) continue;

                var questId = specialOrder.questKey.ToString();
                var flavor = this.questPatchStatusHelper.GetPatchingFlavor(questId);
                this.ModifyLiveSpecialOrder(questId, specialOrder, flavor);
            }
        }
        private void GameLoop_SaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            this.OverrideQuestRewards();
        }
        #endregion "Live Data Replacing"

    }
}