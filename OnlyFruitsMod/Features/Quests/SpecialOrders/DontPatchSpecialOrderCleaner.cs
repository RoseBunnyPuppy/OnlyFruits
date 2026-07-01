using StardewValley.GameData.SpecialOrders;
using StardewValley.SpecialOrders;

namespace OnlyFruitsMod.Features.Quests.SpecialOrders
{
    public class DontPatchSpecialOrderCleaner : ISpecialOrderCleaner
    {
        public void PatchAsset(SpecialOrderData? specialOrderData)
        {
            // NOTE: this does nothing by design.
        }

        public void PatchLiveData(SpecialOrder specialOrder)
        {
            QuestRewardHelper.Instance.RestoreMoneyReward(specialOrder);
            QuestRewardHelper.Instance.RestoreGemReward(specialOrder);
        }
    }
}
