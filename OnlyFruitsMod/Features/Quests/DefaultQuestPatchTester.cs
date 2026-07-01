using OnlyFruitsMod.ModParts.Models;

namespace OnlyFruitsMod.Features.Quests
{
    public class DefaultQuestPatchTester : IQuestPatchTester
    {
        public ModPartContext Context { get; }
        public HashSet<string> FruityQuestIds { get; }

        public DefaultQuestPatchTester(
            ModPartContext context
        )
        {
            Context = context;
            this.FruityQuestIds = this.Context.Helper.ModContent.Load<string[]>("assets/fruity_quest_ids.json").ToHashSet();
        }


        public bool IsPatchingDailyQuest()
        {
            // dont patch if the challenge isnt enabled.
            if (!this.Context.PerSaveChallengeInstance.IsChallengeEnabled) return false;

            // dont patch if we are restoring the quest rewards
            if (this.Context.ConfigInstance.Config.RestoreAllQuestRewards) return false;

            // dont patch if we arent restricting non-fruity quests
            else if (!this.Context.ConfigInstance.Config.Questing_NoMoneyFromNonFruityQuests) return false;

            return true;
        }
        public bool IsPatchingQuest(string questId)
        {
            // dont patch if the challenge isnt enabled.
            if (!this.Context.PerSaveChallengeInstance.IsChallengeEnabled) return false;

            // dont patch if we are restoring the quest rewards
            if (this.Context.ConfigInstance.Config.RestoreAllQuestRewards) return false;

            // dont patch if we arent restricting non-fruity quests
            else if (!this.Context.ConfigInstance.Config.Questing_NoMoneyFromNonFruityQuests) return false;
            // if it is a fruity quest, we arent patching
            else if (FruityQuestIds.Contains(questId)) return false;
            return true;
        }
    }
   
}
