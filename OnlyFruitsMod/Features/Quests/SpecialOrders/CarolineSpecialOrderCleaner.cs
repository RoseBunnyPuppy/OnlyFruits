using OnlyFruitsMod.Extensions;
using OnlyFruitsMod.Features.Logging;
using OnlyFruitsMod.Infrastructure;
using StardewModdingAPI;
using StardewValley.GameData.SpecialOrders;
using StardewValley.SpecialOrders;

namespace OnlyFruitsMod.Features.Quests.SpecialOrders
{
    public class CarolineSpecialOrderCleaner : ISpecialOrderCleaner
    {
        public void PatchAsset(SpecialOrderData? specialOrderData)
        {
            if (specialOrderData == null) return;

            var randomizedCrops = specialOrderData.RandomizedElements.GetByName(HardcodedQuestConstants.RandomizedElementNames.Crop);
            if (randomizedCrops == null) return;
            var element = randomizedCrops.Values.Single();
            element.Value = PickItemHelper.Instance.Serialize(new string[] {
                "Pineapple",
            });
        }

        public void PatchLiveData(SpecialOrder specialOrder)
        {
            const string PineappleCropId = "(O)832";
            if (!specialOrder.preSelectedItems.TryGetValue(HardcodedQuestConstants.RandomizedElementNames.Crop, out var selectedCrop) || selectedCrop == null)
            {
                Logger.Instance.Monitor.Log("Failed to find the pre-selected crop for Caroline's quest", LogLevel.Error);
                return;
            }
            if (selectedCrop == PineappleCropId) return;
            specialOrder.preSelectedItems[HardcodedQuestConstants.RandomizedElementNames.Crop] = PineappleCropId;
        }
    }
}
