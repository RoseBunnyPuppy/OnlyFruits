using OnlyFruitsMod.Extensions;
using OnlyFruitsMod.Features.Logging;
using OnlyFruitsMod.Features.Quests.Models;
using OnlyFruitsMod.Features.Quests.SpecialOrders;
using OnlyFruitsMod.Features.ReloadHelpers;
using OnlyFruitsMod.Infrastructure;
using OnlyFruitsMod.ModParts.Core;
using OnlyFruitsMod.ModParts.Models;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.SpecialOrders;

namespace OnlyFruitsMod.ModParts
{
    /// <summary>
    ///   The special-order modification code.
    /// </summary>
    public class SpecialOrderModPart : ModPartBase
    {
        public bool PreloadAssets { get; set; } = PreloadConfiguration.SpecialOrders;


        private readonly SpecialOrderCleanerDriver specialOrderCleaner = new();

        private readonly ReloadManager reloadManager = new();
        private readonly SpecialOrderStatusDeterminer questPatchStatusHelper;

        public SpecialOrderKeysConfigModel SpecialOrderKeysConfigModel { get; }

        /// <summary>
        ///   The cached version of the SpecialOrderStrings.
        /// </summary>
        private Dictionary<string, string> CachedSpecialOrderStrings { get; set; } = new Dictionary<string, string>();

        private record SpecialOrderReward(SpecialOrderData SpecialOrder, SpecialOrderRewardData RewardData);

        public SpecialOrderModPart(
            ModPartContext context
        ) : base(context)
        {
            // load config definition assets
            this.SpecialOrderKeysConfigModel = this.helper.ModContent.Load<SpecialOrderKeysConfigModel>("assets/fruity_special_order_keys.json");

            // 
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
            helper.Events.Content.AssetReady += Content_AssetReady;
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

        private void Content_AssetReady(object? sender, AssetReadyEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataSpecialOrders))
            {
                Logger.Instance.LogAssetReady(this, e.NameWithoutLocale);
                // re-apply the live data changes if needed
                if (this.reloadManager.ConsumeReload())
                {
                    this.OverrideQuestRewards();
                }
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
                        this.specialOrderCleaner.PatchAsset(specialOperationData, flavor);
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

       
       
        #endregion "Asset Patching"


        #region "Live Data Replacing"


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
                this.specialOrderCleaner.PatchLiveData(specialOrder, flavor);
            }
        }
        private void GameLoop_SaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            this.OverrideQuestRewards();
        }
        #endregion "Live Data Replacing"

    }
}