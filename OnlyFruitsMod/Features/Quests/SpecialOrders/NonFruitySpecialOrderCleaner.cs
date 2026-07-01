using StardewValley.GameData.SpecialOrders;
using StardewValley.SpecialOrders;

namespace OnlyFruitsMod.Features.Quests.SpecialOrders
{
    public class NonFruitySpecialOrderCleaner : ISpecialOrderCleaner
    {
        public void PatchAsset(SpecialOrderData? specialOrderData)
        {
            if (specialOrderData == null) return;
            QuestRewardHelper.Instance.SetMoneyRewardToZero(specialOrderData);
        }
        public void PatchLiveData(SpecialOrder specialOrder)
        {
            QuestRewardHelper.Instance.SetMoneyRewardToZero(specialOrder);
        }
    }
}
