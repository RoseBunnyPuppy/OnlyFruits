using Netcode;
using OnlyFruitsMod.Extensions;
using OnlyFruitsMod.Infrastructure;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyFruitsMod.Features.Quests
{
    public class DailyQuestPatcher
    {
        public bool IsVerbose { get; set; } = true;

        private readonly IMonitor monitor;
        private readonly IQuestPatchTester questPatchTester;
        private readonly IDailyQuestRegenerator dailyQuestRegenerator;

        public DailyQuestPatcher(
            IMonitor monitor,
            IQuestPatchTester questPatchTester,
            IDailyQuestRegenerator dailyQuestRegenerator
        )
        {
            this.monitor = monitor;
            this.questPatchTester = questPatchTester;
            this.dailyQuestRegenerator = dailyQuestRegenerator;
        }

     


        public void AutoPatchHelpWantedQuest(IDictionary<string, string>? questData)
        {
            if (!Game1.CanAcceptDailyQuest()) return;
            var quest = Game1.questOfTheDay;
            if (quest == null) return;
            if (questData == null) return;

            if (quest.id.Value == null)
            {
                if (!this.questPatchTester.IsPatchingDailyQuest())
                {
                    if (!quest.IsOnlyFruitsQOTD()) return;
                    this.dailyQuestRegenerator.RegenerateQuest(quest, false);
                    return;
                }

                if (quest.IsOnlyFruitsQOTD()) return;
                this.dailyQuestRegenerator.RegenerateQuest(quest, true);
                return;
            }
            else
            {
                // skip if it is a known quest of the day
                if (quest.IsOnlyFruitsQOTD()) return;

                // TODO: here was where the quest had a null id ("Bring Haley Aquamarine.")
                // skip if the quest isnt known
                if (!questData.TryGetValue(quest.id.Value, out _)) return;

                // restore the rewards if we arent patching
                if (!this.questPatchTester.IsPatchingQuest(quest.id.Value))
                {
                    if (this.IsVerbose) this.monitor.LogDebug($"Patching {quest.GetDescription()}");
                    // if this quest has a numeric reward, dont overwrite it
                    if (quest.moneyReward.Value != 0) return;

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
            }
        }

    }
   
}
