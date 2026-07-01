using StardewValley.GameData.SpecialOrders;
using StardewValley.SpecialOrders;

namespace OnlyFruitsMod.Features.Quests.SpecialOrders
{
    public class NonFruityQiSpecialOrderCleaner : ISpecialOrderCleaner
    {
        public void PatchAsset(SpecialOrderData? specialOrderData)
        {
            if (specialOrderData == null) return;
            QuestRewardHelper.Instance.SetGemRewardToZero(specialOrderData);
            QuestRewardHelper.Instance.SetMoneyRewardToZero(specialOrderData);
        }

        public void PatchLiveData(SpecialOrder specialOrder)
        {
            QuestRewardHelper.Instance.SetMoneyRewardToZero(specialOrder);
            QuestRewardHelper.Instance.SetGemRewardToZero(specialOrder);
        }
    }
}
