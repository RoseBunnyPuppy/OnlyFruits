using Netcode;
using OnlyFruitsMod.Extensions;
using OnlyFruitsMod.Features.Fruits;
using OnlyFruitsMod.Features.ModConfiguration;
using OnlyFruitsMod.Features.Quests.Models;
using OnlyFruitsMod.Features.ReloadHelpers;
using OnlyFruitsMod.Infrastructure;
using OnlyFruitsMod.ModParts.Core;
using OnlyFruitsMod.ModParts.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Shops;
using StardewValley.GameData.SpecialOrders;
using StardewValley.Quests;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace OnlyFruitsMod.ModParts
{
    public class QuestModPart : ModPartBase
    {

        public bool IsVerbose { get; set; } = false;
        private readonly ReloadManager reloadManager = new();
        public bool PreloadAssets { get; set; } = PreloadConfiguration.Quests;
        public IDictionary<string, string>? CachedQuestData { get; private set; }
        private HashSet<string> FruityQuestIds { get; }

        public QuestModPart(
            ModPartContext context
        ) : base(context)
        {
            this.FruityQuestIds = this.helper.ModContent.Load<string[]>("assets/fruity_quest_ids.json").ToHashSet();
            
            if (this.IsVerbose) this.monitor.LogDebug($"[{this.GetType().Name}] EnqueueingReload FirstLoad.");
            this.reloadManager.EnqueueReload();
        }

        private bool IsPatchingDailyQuest()
        {
            // dont patch if the challenge isnt enabled.
            if (!this.Context.PerSaveChallengeInstance.IsChallengeEnabled) return false;

            // dont patch if we are restoring the quest rewards
            if (this.configInstance.Config.RestoreAllQuestRewards) return false;

            // dont patch if we arent restricting non-fruity quests
            else if (!this.configInstance.Config.Questing_NoMoneyFromNonFruityQuests) return false;

            return true;
        }
        private bool IsPatchingQuest(string questId)
        {
            // dont patch if the challenge isnt enabled.
            if (!this.Context.PerSaveChallengeInstance.IsChallengeEnabled) return false;

            // dont patch if we are restoring the quest rewards
            if (this.configInstance.Config.RestoreAllQuestRewards) return false;

            // dont patch if we arent restricting non-fruity quests
            else if (!this.configInstance.Config.Questing_NoMoneyFromNonFruityQuests) return false;
            // if it is a fruity quest, we arent patching
            else if (FruityQuestIds.Contains(questId)) return false;
            return true;
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
                this.monitor.LogAssetReady(this, e.NameWithoutLocale);
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
                        if (!this.IsPatchingQuest(kvp.Key)) continue;
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

        private bool RestoreReward(Quest quest, NetInt rewardField)
        {
            // abort if there is no directly stored cached reward
            if (!quest.TryGetDirectlyCachedModDataQuestReward(out var directReward)) return false;

            // otherwise, apply the cached value
            rewardField.Value = directReward;
            return true;

        }
        private void SetOnlyFruitsQOTDStatus(Quest quest) =>
            quest.modData[HardcodedModDataKeys.IsOnlyFruitsQuestOfTheDay] = "true";

        private void ConfigureOnlyFruitsQOTD(Quest quest, bool markAsOnlyFruitQuest)
        {
            quest.dailyQuest.Value = true;
            if (markAsOnlyFruitQuest)
            {
                this.SetOnlyFruitsQOTDStatus(quest);
            }
            Game1.netWorldState.Value.SetQuestOfTheDay(quest);
        }
        private bool IsOnlyFruitsQOTD(Quest quest) => quest.modData.TryGetValue(HardcodedModDataKeys.IsOnlyFruitsQuestOfTheDay, out _);
        private Quest RegenerateQuest(Quest quest, bool markAsOnlyFruitQuest)
        {
            if (quest is ItemDeliveryQuest _)
            {
                var freshQuest = new ItemDeliveryQuest();
                freshQuest.loadQuestInfo();
                this.ConfigureOnlyFruitsQOTD(freshQuest, markAsOnlyFruitQuest);
                freshQuest.accepted.Value = quest.accepted.Value;
                return freshQuest;
            }
            else if (quest is FishingQuest _)
            {
                var freshQuest = new FishingQuest();
                freshQuest.loadQuestInfo();
                this.ConfigureOnlyFruitsQOTD(freshQuest, markAsOnlyFruitQuest);
                freshQuest.accepted.Value = quest.accepted.Value;
                return freshQuest;
            }
            else if (quest is SlayMonsterQuest _)
            {
                var freshQuest = new SlayMonsterQuest();
                freshQuest.loadQuestInfo();
                this.ConfigureOnlyFruitsQOTD(freshQuest, markAsOnlyFruitQuest);
                freshQuest.accepted.Value = quest.accepted.Value;
                return freshQuest;
            }
            else if (quest is ResourceCollectionQuest _)
            {
                var freshQuest = new ResourceCollectionQuest();
                freshQuest.loadQuestInfo();
                this.ConfigureOnlyFruitsQOTD(freshQuest, markAsOnlyFruitQuest);
                freshQuest.accepted.Value = quest.accepted.Value;
                return freshQuest;
            }
            return quest;
        }
        private void AutoPatchHelpWantedQuest(IDictionary<string, string>? questData)
        {
            if (!Game1.CanAcceptDailyQuest()) return;
            var quest = Game1.questOfTheDay;
            if (quest == null) return;
            if (questData == null) return;

            if (quest.id.Value == null)
            {
                if (!this.IsPatchingDailyQuest())
                {
                    if (!this.IsOnlyFruitsQOTD(quest)) return;
                    this.RegenerateQuest(quest, false);
                    return;
                }

                if (this.IsOnlyFruitsQOTD(quest)) return;
                this.RegenerateQuest(quest, true);
                return;
            }
            else
            {
                // skip if it is a known quest of the day
                if (this.IsOnlyFruitsQOTD(quest)) return;

                // TODO: here was where the quest had a null id ("Bring Haley Aquamarine.")
                // skip if the quest isnt known
                if (!questData.TryGetValue(quest.id.Value, out _)) return;

                // restore the rewards if we arent patching
                if (!this.IsPatchingQuest(quest.id.Value))
                {
                    if (this.IsVerbose) this.monitor.LogDebug($"Patching {quest.GetDescription()}");
                    // if this quest has a numeric reward, dont overwrite it
                    if (quest.moneyReward.Value != 0) return;

                    this.RestoreReward(quest, quest.moneyReward);
                }
                else
                {
                    if (this.IsVerbose) this.monitor.LogDebug($"Not Patching {quest.GetDescription()}");
                    // cache the original reward data
                    quest.TrySetCachedModDataQuestReward(quest.moneyReward.Value);

                    // clear the reward
                    quest.moneyReward.Value = 0;
                }
            }
        }


        private bool AutoPatchLiveQuest(
            Quest? quest,
            IDictionary<string, string>? questData,
            [NotNullWhen(returnValue: true)] out Quest? regeneratedQuest
        )
        {
            regeneratedQuest = default;
            if (quest == null) return false;
            if (questData == null) return false;


            // handle patched 'quest of the day's
            if (this.IsOnlyFruitsQOTD(quest))
            {
                // if we are supposed to patch it, then do nothing
                if (this.IsPatchingDailyQuest()) return false;

                // otherwise, reset the qotd
                regeneratedQuest = this.RegenerateQuest(quest, false);
                return true;
            }
            // if there is no id, it is a qotd
            else if (quest.id.Value == null)
            {
                // if we arent supposed to patch it, then do nothing
                if (!this.IsPatchingDailyQuest()) return false;

                regeneratedQuest = this.RegenerateQuest(quest, true);
                return true;
            }

            // skip if the quest isnt known
            if (!questData.TryGetValue(quest.id.Value, out _)) return false;

            // restore the rewards if we arent patching
            if (!this.IsPatchingQuest(quest.id.Value))
            {
                if (this.IsVerbose) this.monitor.LogDebug($"Patching {quest.GetDescription()}");
                // if this quest has a numeric reward, dont overwrite it
                if (quest.moneyReward.Value != 0) return false;

                this.RestoreReward(quest, quest.moneyReward);
            }
            else
            {
                if (this.IsVerbose) this.monitor.LogDebug($"Not Patching {quest.GetDescription()}");
                // cache the original reward data
                quest.TrySetCachedModDataQuestReward(quest.moneyReward.Value);

                // clear the reward
                quest.moneyReward.Value = 0;
            }
            return false;
        }
        private void OverrideExistingLiveData()
        {
            // abort if we arent in a game
            if (!Game1.hasLoadedGame) return;

            // do nothing if we dont currently have any per-save data
            if (!this.Context.PerSaveChallengeInstance.HasPerSaveLoaded) return;

            if (!this.reloadManager.ShouldApplyLiveData)
            {
                if (this.IsVerbose) this.monitor.LogDebug($"[{this.GetType().Name}] We arent marked as 'able to override' yet, so reload!");
                this.Context.Helper.GameContent.LoadAsset(HardcodedAssetPaths.DataQuests);
                return;
            }

            if (this.IsVerbose) this.monitor.LogDebug($"[{this.GetType().Name}] Got beyond the 'ShouldApply' check");
            var questData = this.CachedQuestData;
            if (questData == null) return;



            this.AutoPatchHelpWantedQuest(questData);

            for (int i = 0; i < Game1.player.questLog.Count; i++)
            {
                var quest = Game1.player.questLog[i];
                if (!this.AutoPatchLiveQuest(quest, questData, out var regeneratedQuest)) continue;
                Game1.player.questLog[i] = regeneratedQuest;
            }

        }
       
        private void GameLoop_DayStarted(object? sender, DayStartedEventArgs e)
        {
            if (this.IsVerbose) this.monitor.LogDebug("Day started!");
            // abort if we arent in a game
            if (!Game1.hasLoadedGame) return;

            // do nothing if we dont currently have any per-save data
            if (!this.Context.PerSaveChallengeInstance.HasPerSaveLoaded) return;

            if (!this.reloadManager.ShouldApplyLiveData)
            {
                if (this.IsVerbose) this.monitor.LogDebug($"[{this.GetType().Name}] We arent marked as 'able to override' yet, so reload!");
                this.Context.Helper.GameContent.LoadAsset(HardcodedAssetPaths.DataQuests);
                return;
            }

            if (this.IsVerbose) this.monitor.LogDebug($"[{this.GetType().Name}] Got beyond the 'ShouldApply' check");
            var questData = this.CachedQuestData;
            if (questData == null) return;

            this.AutoPatchHelpWantedQuest(questData);
        }

        private void GameLoop_SaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            this.OverrideExistingLiveData();
        }
        #endregion "Live Data Replacing"

    }
}
