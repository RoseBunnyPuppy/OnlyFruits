using StardewValley.GameData.SpecialOrders;
using StardewValley.SpecialOrders;

namespace OnlyFruitsMod.Features.Quests.SpecialOrders
{
    public interface ISpecialOrderCleanerDriver
    {
        /// <summary>
        ///     Patch the <paramref name="specialOrderData"/> asset using the 
        ///   appropriate cleaner for the given <paramref name="flavor"/>.
        /// </summary>
        void PatchAsset(SpecialOrderData? specialOrderData, OrderPatchingFlavors flavor);

        /// <summary>
        ///     Patch the <paramref name="specialOrder"/> live data using the 
        ///   appropriate cleaner for the given <paramref name="flavor"/>.
        /// </summary>
        void PatchLiveData(SpecialOrder? specialOrder, OrderPatchingFlavors flavor);
    }
}
