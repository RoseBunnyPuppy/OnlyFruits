using OnlyFruitsMod.Features.Logging;
using StardewModdingAPI;
using StardewValley.GameData.SpecialOrders;
using StardewValley.SpecialOrders;

namespace OnlyFruitsMod.Features.Quests.SpecialOrders
{
    public class SpecialOrderCleanerDriver : ISpecialOrderCleanerDriver
    {
        private readonly Dictionary<OrderPatchingFlavors, ISpecialOrderCleaner> flavoredSpecialOrderCleaners = new()
        {
            [OrderPatchingFlavors.CarolineSpecialOrder] = new CarolineSpecialOrderCleaner(),
            [OrderPatchingFlavors.LewisSpecialOrder] = new LewisSpecialOrderCleaner(),
            [OrderPatchingFlavors.NonFruityQi] = new NonFruityQiSpecialOrderCleaner(),
            [OrderPatchingFlavors.PotentiallyNonFruityQi] = new NonFruityQiSpecialOrderCleaner(),
            [OrderPatchingFlavors.NonFruity] = new NonFruitySpecialOrderCleaner(),
            [OrderPatchingFlavors.DontPatch] = new DontPatchSpecialOrderCleaner(),

        };
        /// <inheritdoc />
        public void PatchAsset(SpecialOrderData? specialOrderData, OrderPatchingFlavors flavor)
        {
            if (this.flavoredSpecialOrderCleaners.TryGetValue(flavor, out var cleaner))
            {
                cleaner.PatchAsset(specialOrderData);
                return;
            }
            else
            {
                Logger.Instance.Monitor.Log($"Unsuppored asset patch status '{flavor}'.  Treating as 'do not patch'", LogLevel.Error);
                return;
            }
        }

        /// <inheritdoc />
        public void PatchLiveData(SpecialOrder? specialOrder, OrderPatchingFlavors flavor)
        {
            if (specialOrder == null) return;
            if (this.flavoredSpecialOrderCleaners.TryGetValue(flavor, out var cleaner))
            {
                cleaner.PatchLiveData(specialOrder);
                return;
            }
            else
            {
                Logger.Instance.Monitor.Log($"Unsuppored live patch status '{flavor}'.  Treating as 'do not patch'", LogLevel.Error);
                return;
            }
        }
    }
}
