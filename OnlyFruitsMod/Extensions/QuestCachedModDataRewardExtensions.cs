using OnlyFruitsMod.Infrastructure;
using StardewValley;
using StardewValley.Quests;

namespace OnlyFruitsMod.Extensions
{
    public static class QuestCachedModDataRewardExtensions
    {
        /// <summary>
        ///     Attempt to set the original money-reward within the 
        ///   quest's modData.  If <paramref name="force"/> is
        ///   false, the value will only be set if it is
        ///   not already set.
        /// </summary>
        public static bool TrySetCachedModDataQuestReward(this Quest quest, int value, bool force = false)
        {
            // if there is no price stored in the mod data, update it
            if (force || !quest.modData.TryGetValue(HardcodedModDataKeys.OriginalQuestRewardModDataKey, out _))
            {
                quest.modData[HardcodedModDataKeys.OriginalQuestRewardModDataKey] = value.ToString();
                return true;
            }
            return false;
        }

        public static bool TryGetDirectlyCachedModDataQuestReward(this Quest quest, out int value)
        {
            value = default;
            // fail if not set
            if (!quest.modData.TryGetValue(HardcodedModDataKeys.OriginalQuestRewardModDataKey, out var currentValue)) return false;

            return int.TryParse(currentValue, out value);
        }
    }
}
