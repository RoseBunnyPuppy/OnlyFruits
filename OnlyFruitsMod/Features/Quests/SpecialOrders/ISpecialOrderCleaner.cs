using StardewValley.GameData.SpecialOrders;
using StardewValley.SpecialOrders;

namespace OnlyFruitsMod.Features.Quests.SpecialOrders
{
    public interface ISpecialOrderCleaner
    {
        void PatchAsset(SpecialOrderData? specialOrderData);
        void PatchLiveData(SpecialOrder specialOrder);
    }

}
