using OnlyFruitsMod.Extensions;
using OnlyFruitsMod.Features.Logging;
using OnlyFruitsMod.Features.Quests;
using OnlyFruitsMod.Features.Quests.Models;
using OnlyFruitsMod.Features.ReloadHelpers;
using OnlyFruitsMod.Infrastructure;
using OnlyFruitsMod.ModParts.Core;
using OnlyFruitsMod.ModParts.Models;
using StardewModdingAPI.Events;
using StardewValley;

namespace OnlyFruitsMod.ModParts
{
    public class QuestModPart : ModPartBase
    {
        private readonly IQuestPatchTester questPatchTester;
        private readonly IDailyQuestRegenerator dailyQuestRegenerator;
        private readonly LiveQuestPatcher liveQuestPatcher;
        private readonly DailyQuestPatcher dailyQuestPatcher;

        private readonly ReloadManager reloadManager = new();
        public bool PreloadAssets { get; set; } = PreloadConfiguration.Quests;
        public IDictionary<string, string>? CachedQuestData { get; private set; }

        public QuestModPart(
            ModPartContext context
        ) : base(context)
        {
            this.questPatchTester = new DefaultQuestPatchTester(context);
            this.dailyQuestRegenerator = new DailyQuestRegenerator();
            this.liveQuestPatcher = new(
                this.monitor,
                this.questPatchTester,
                dailyQuestRegenerator
            );
            this.dailyQuestPatcher = new(
                context.Monitor,
                this.questPatchTester,
                dailyQuestRegenerator
            );
            
            this.reloadManager.EnqueueReload();
        }

      

        /// <inheritdoc/>
        protected override void AttachListeners()
        {
            base.AttachListeners();
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted; ;
            helper.Events.GameLoop.SaveLoaded += this.GameLoop_SaveLoaded;
            helper.Events.Content.AssetReady += Content_AssetReady; ;
        }

        /// <summary>
        ///     When the configuration changes, invalidate the assets
        ///   and re-initialize the live data once the assets have 
        ///   reloaded.
        /// </summary>
        protected override void OnModConfigChanged(object? sender, EventArgs e)
        {
            this.reloadManager.EnqueueReload();
            this.helper.GameContent.InvalidateCache(HardcodedAssetPaths.DataQuests);
            this.helper.GameContent.LoadAsset(HardcodedAssetPaths.DataQuests);
        }

        /// <inheritdoc/>
        protected override void LoadNeededAssets()
        {
            // dont pre-load shit unless needed
            if (!this.PreloadAssets) return;

            this.helper.GameContent.LoadAsset(HardcodedAssetPaths.DataQuests);
        }


        #region "Asset Patching"
        private void Content_AssetReady(object? sender, AssetReadyEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataQuests))
            {
                Logger.Instance.LogAssetReady(this, e.NameWithoutLocale);
                // re-apply the live data changes if needed
                if (this.reloadManager.ConsumeReload())
                {
                    this.OverrideExistingLiveData();
                }
            }
        }
        protected override void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(HardcodedAssetPaths.DataQuests))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsAutoDictionary(HardcodedAssetPaths.DataQuests).Data;
                    this.CachedQuestData = data;

                    foreach (var kvp in data)
                    {
                        // dont patch if not needed
                        if (!this.questPatchTester.IsPatchingQuest(kvp.Key)) continue;
                        var parsed = ParsedQuest.Parse(kvp.Value);

                        // skip if no money is given as a reward
                        var hasMoneyAsReward = int.Parse(parsed.MoneyReward) > 0;
                        if (!hasMoneyAsReward) continue;

                        parsed.MoneyReward = "0";
                        data[kvp.Key] = parsed.Serialize();
                    }
                });
                return;
            }
        }
        #endregion "Asset Patching"


        #region "Live Data Replacing"
      

       
        private void OverrideExistingLiveData()
        {
            // abort if we arent in a game
            if (!Game1.hasLoadedGame) return;

            // do nothing if we dont currently have any per-save data
            if (!this.Context.PerSaveChallengeInstance.HasPerSaveLoaded) return;

            if (!this.reloadManager.ShouldApplyLiveData)
            {
                this.Context.Helper.GameContent.LoadAsset(HardcodedAssetPaths.DataQuests);
                return;
            }

            var questData = this.CachedQuestData;
            if (questData == null) return;



            this.dailyQuestPatcher.AutoPatchHelpWantedQuest(questData);

            for (int i = 0; i < Game1.player.questLog.Count; i++)
            {
                var quest = Game1.player.questLog[i];
                if (!this.liveQuestPatcher.AutoPatchLiveQuest(quest, questData, out var regeneratedQuest)) continue;
                Game1.player.questLog[i] = regeneratedQuest;
            }

        }
       
        private void GameLoop_DayStarted(object? sender, DayStartedEventArgs e)
        {
            // abort if we arent in a game
            if (!Game1.hasLoadedGame) return;

            // do nothing if we dont currently have any per-save data
            if (!this.Context.PerSaveChallengeInstance.HasPerSaveLoaded) return;

            if (!this.reloadManager.ShouldApplyLiveData)
            {
                this.Context.Helper.GameContent.LoadAsset(HardcodedAssetPaths.DataQuests);
                return;
            }

            var questData = this.CachedQuestData;
            if (questData == null) return;

            this.dailyQuestPatcher.AutoPatchHelpWantedQuest(questData);
        }

        private void GameLoop_SaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            this.OverrideExistingLiveData();
        }
        #endregion "Live Data Replacing"

    }
}
