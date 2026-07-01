using Netcode;
using OnlyFruitsMod.Extensions;
using StardewValley.Quests;

namespace OnlyFruitsMod.Features.Quests
{
    public interface IDailyQuestRegenerator
    {
        bool RestoreReward(Quest quest, NetInt rewardField);
        
        Quest RegenerateQuest(Quest quest, bool markAsOnlyFruitQuest);
    }
   
}
