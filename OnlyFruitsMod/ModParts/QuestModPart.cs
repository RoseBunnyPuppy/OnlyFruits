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

namespace OnlyFruitsMod.ModParts
{
    public class QuestModPart : ModPartBase
    {
        private readonly ReloadManager reloadManager = new();
        public bool PreloadAssets { get; set; } = PreloadConfiguration.Quests;
        public IDictionary<string, string>? CachedQuestData { get; private set; }
        private HashSet<string> FruityQuestIds { get; }

        public QuestModPart(
            ModPartContext context
        ) : base(context)
        {
            this.FruityQuestIds = this.helper.ModContent.Load<string[]>("assets/fruity_quest_ids.json").ToHashSet();
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
            helper.Events.GameLoop.SaveLoaded += this.GameLoop_SaveLoaded;
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
        }

        /// <inheritdoc/>
        protected override void LoadNeededAssets()
        {
            // dont pre-load shit unless needed
            if (!this.PreloadAssets) return;

            this.helper.GameContent.LoadAsset(HardcodedAssetPaths.DataQuests);
        }


        #region "Asset Patching"
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
                        if (this.IsPatchingQuest(kvp.Key)) continue;
                        
                        var parsed = ParsedQuest.Parse(kvp.Value);

                        // skip if no money is given as a reward
                        var hasMoneyAsReward = int.Parse(parsed.MoneyReward) > 0;
                        if (!hasMoneyAsReward) continue;

                        parsed.MoneyReward = "0";
                        data[kvp.Key] = parsed.Serialize();
                    }
                });

                // re-apply the live data changes if needed
                if (this.reloadManager.ConsumeReload())
                {
                    this.OverrideExistingLiveData();
                }
                return;
            }
        }
        #endregion "Asset Patching"


        #region "Live Data Replacing"

        private bool RestoreReward(Quest quest, NetInt rewardField)
        {
            // if there is a directly stored cached reward, apply it
            if (quest.TryGetDirectlyCachedModDataQuestReward(out var directReward))
            {
                rewardField.Value = directReward;
                return true;
            }

            // TODO: otherwise, try to apply the asset reward

            return false;

        }
        private void OverrideExistingLiveData()
        {
            // abort if we arent in a game
            if (!Game1.hasLoadedGame) return;
            
            // do nothing if we dont currently have any per-save data
            if (!this.Context.PerSaveChallengeInstance.HasPerSaveLoaded) return;

            var questData = this.CachedQuestData;
            if (questData == null) return;

            foreach (var quest in Game1.player.questLog)
            {
                if (quest == null) continue;

                // skip if the quest isnt known
                if (!questData.TryGetValue(quest.id.Value, out _)) continue;

                // restore the rewards if we arent patching
                if (!this.IsPatchingQuest(quest.id.Value))
                {
                    // if this quest has a numeric reward, dont overwrite it
                    if (quest.moneyReward.Value != 0) continue;

                    this.RestoreReward(quest, quest.moneyReward);
                    continue;
                }
                else
                {
                    // cache the original reward data
                    quest.TrySetCachedModDataQuestReward(quest.moneyReward.Value);
                    
                    // clear the reward
                    quest.moneyReward.Value = 0;
                }

            }
        }


        private void GameLoop_SaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            this.OverrideExistingLiveData();
        }
        #endregion "Live Data Replacing"

    }
}
