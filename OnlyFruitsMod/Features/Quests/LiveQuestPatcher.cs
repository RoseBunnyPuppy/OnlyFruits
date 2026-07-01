using OnlyFruitsMod.Extensions;
using StardewModdingAPI;
using StardewValley.Quests;
using System.Diagnostics.CodeAnalysis;

namespace OnlyFruitsMod.Features.Quests
{
    public class LiveQuestPatcher
    {
        public bool IsVerbose { get; set; } = true;
        private readonly IMonitor monitor;
        private readonly IQuestPatchTester questPatchTester;
        private readonly IDailyQuestRegenerator dailyQuestRegenerator;

        public LiveQuestPatcher(
            IMonitor monitor,
            IQuestPatchTester questPatchTester,
            IDailyQuestRegenerator dailyQuestRegenerator
        )
        {
            this.monitor = monitor;
            this.questPatchTester = questPatchTester;
            this.dailyQuestRegenerator = dailyQuestRegenerator;
        }

        public bool AutoPatchLiveQuest(
            Quest? quest,
            IDictionary<string, string>? questData,
            [NotNullWhen(returnValue: true)] out Quest? regeneratedQuest
        )
        {
            regeneratedQuest = default;
            if (quest == null) return false;
            if (questData == null) return false;


            // handle patched 'quest of the day's
            if (quest.IsOnlyFruitsQOTD())
            {
                // if we are supposed to patch it, then do nothing
                if (this.questPatchTester.IsPatchingDailyQuest())
                {
                    this.monitor.LogDebug($"old: OF -> new: OF.  Doing nothing");
                    return false;
                }

                // otherwise, reset the qotd
                this.monitor.LogDebug($"old: OF -> new: real.  regenerating [false]");
                regeneratedQuest = this.dailyQuestRegenerator.RegenerateQuest(quest, false);
                return true;
            }
            // if there is no id, it is a qotd
            else if (quest.id.Value == null)
            {
                // if we arent supposed to patch it, then do nothing
                if (!this.questPatchTester.IsPatchingDailyQuest())
                {
                    this.monitor.LogDebug($"old: real -> new: real.  Doing nothing");
                    return false;
                }

                this.monitor.LogDebug($"old: real -> new: OF.  regenerating [true]");
                regeneratedQuest = this.dailyQuestRegenerator.RegenerateQuest(quest, true);
                return true;
            }

            // skip if the quest isnt known
            if (!questData.TryGetValue(quest.id.Value, out _)) return false;

            // restore the rewards if we arent patching
            if (!this.questPatchTester.IsPatchingQuest(quest.id.Value))
            {
                if (this.IsVerbose) this.monitor.LogDebug($"Patching {quest.GetDescription()}");
                // if this quest has a numeric reward, dont overwrite it
                if (quest.moneyReward.Value != 0) return false;

                this.dailyQuestRegenerator.RestoreReward(quest, quest.moneyReward);
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
    }
   
}
