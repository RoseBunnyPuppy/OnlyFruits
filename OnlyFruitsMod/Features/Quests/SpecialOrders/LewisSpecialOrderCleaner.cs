using OnlyFruitsMod.Extensions;
using OnlyFruitsMod.Features.ItemIds;
using OnlyFruitsMod.Features.Logging;
using OnlyFruitsMod.Features.Prices;
using OnlyFruitsMod.Infrastructure;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.SpecialOrders;
using StardewValley.SpecialOrders;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace OnlyFruitsMod.Features.Quests.SpecialOrders
{
    public class LewisSpecialOrderCleaner : ISpecialOrderCleaner
    {
        const string CropPrefix = ItemIdPrefixes.Objects;
        private enum LewisSeasons
        {
            Spring,
            Summer,
            Fall,
        }
        private record SeasonStatusPair(LewisSeasons Season, bool IsFruit, string? Name = default);
        private static Dictionary<string, SeasonStatusPair> DefaultUnmappedCropToSeason { get; } = new()
        {
            // spring
            ["Cauliflower"] = new(LewisSeasons.Spring, false),
            ["Garlic"] = new(LewisSeasons.Spring, false),
            ["Green Bean"] = new(LewisSeasons.Spring, false),
            ["Potato"] = new(LewisSeasons.Spring, false),
            ["400"] = new(LewisSeasons.Spring, true, "Strawberry"),
            // summer
            ["258"] = new(LewisSeasons.Summer, true, "Blueberry"),
            ["260"] = new(LewisSeasons.Summer, true, "Hot Pepper"),
            ["254"] = new(LewisSeasons.Summer, true, "Melon"),
            ["Radish"] = new(LewisSeasons.Summer, false),
            ["Tomato"] = new(LewisSeasons.Summer, false),
            ["Wheat"] = new(LewisSeasons.Summer, false),
            // fall
            ["Amaranth"] = new(LewisSeasons.Fall, false),
            ["274"] = new(LewisSeasons.Fall, false, "Artichoke"),
            ["Bok Choy"] = new(LewisSeasons.Fall, false),
            ["282"] = new(LewisSeasons.Fall, true, "Cranberries"),
            ["Eggplant"] = new(LewisSeasons.Fall, false),
            ["398"] = new(LewisSeasons.Fall, true, "Grape"),
            ["Pumpkin"] = new(LewisSeasons.Fall, false),
            ["Yam"] = new(LewisSeasons.Fall, false),
        };
        private Dictionary<string, SeasonStatusPair> CropToSeasonMap { get; }
        private Dictionary<LewisSeasons, List<string>> SeasonToFruitList { get; }

        public LewisSpecialOrderCleaner()
        {
            var source = DefaultUnmappedCropToSeason;
            this.CropToSeasonMap = new Dictionary<string, SeasonStatusPair>(source);
            this.SeasonToFruitList = CreateSeasonMap(source);
        }

        public void PatchAsset(SpecialOrderData? specialOrderData)
        {
            const string SpringSeasonCropKey = "season_spring";
            const string SummerSeasonCropKey = "season_summer";
            const string FallSeasonCropKey = "season_fall";

            if (specialOrderData == null) return;

            var randomizedCrops = specialOrderData.RandomizedElements.GetByName(HardcodedQuestConstants.RandomizedElementNames.Crop);
            if (randomizedCrops == null) return;

            // option 1: remove 'season_spring' because no fruits in it
            // option 2: Replace with  "PICK_ITEM Strawberry"
           
            if (!randomizedCrops.Values.TryGetSingleByRequiredTag(SpringSeasonCropKey, out var springElement))
            {
                Logger.Instance.Monitor.Log($"Failed to find the 'reuired tag' for season '{SpringSeasonCropKey}' for Lewis's special order", LogLevel.Error);
            }
            else
            {
                springElement.Value = PickItemHelper.Instance.Serialize(new string[] {
                    "Strawberry",
                });
            }


            // (season_summer) Replace with  "PICK_ITEM Blueberry, Melon, Hot Pepper"
            if (!randomizedCrops.Values.TryGetSingleByRequiredTag(SummerSeasonCropKey, out var summerElement))
            {
                Logger.Instance.Monitor.Log($"Failed to find the 'reuired tag' for season '{SummerSeasonCropKey}' for Lewis's special order", LogLevel.Error);
            }
            else
            {
                summerElement.Value = PickItemHelper.Instance.Serialize(new string[] {
                    "Blueberry",
                    "Melon",
                    "Hot Pepper",
                });
            }


            // (season_fall) Replace with "PICK_ITEM Cranberries, Grape"
            if (!randomizedCrops.Values.TryGetSingleByRequiredTag(FallSeasonCropKey, out var fallElement))
            {
                Logger.Instance.Monitor.Log($"Failed to find the 'reuired tag' for season '{FallSeasonCropKey}' for Lewis's special order", LogLevel.Error);
            }
            else
            {
                fallElement.Value = PickItemHelper.Instance.Serialize(new string[] {
                    "Cranberries",
                    "Grape",
                });
            }
        }
        public void PatchLiveData(SpecialOrder specialOrder)
        {
            if (!specialOrder.preSelectedItems.TryGetValue(HardcodedQuestConstants.RandomizedElementNames.Crop, out var selectedCrop) || selectedCrop == null)
            {
                Logger.Instance.Monitor.Log("Failed to find the pre-selected crop for Lewis's quest", LogLevel.Error);
                return;
            }
            if (!this.TryGetFruitCrop(selectedCrop, specialOrder.generationSeed.Value, out var updatedCropKey))
                return;
            specialOrder.preSelectedItems[HardcodedQuestConstants.RandomizedElementNames.Crop] = updatedCropKey;
        }
        private string GetRandomChoice(int seed, string orig, SeasonStatusPair pair)
        {
            if (pair.IsFruit) return orig;
            return this.GetRandomChoice(seed, pair.Season);
        }
        private string GetRandomChoice(int seed, LewisSeasons season)
        {
            var rand = new Random(seed);
            var choices = this.SeasonToFruitList[season];
            return $"{CropPrefix}{choices[rand.Next(choices.Count)]}";
        }

        private bool TryGetFruitCrop(
            string key, 
            int seed, 
            [NotNullWhen(returnValue: true)]out string? fruitCrop
        )
        {
            // get the partial id for the given item
            if (!PrefixStripper.Instance.TryStripPrefix(key, CropPrefix, out var sansPrefix))
            {
                Logger.Instance.Monitor.Log($"Failed to get the partial id for an id {key}", LogLevel.Error);
                fruitCrop = default;
                return false;
            }

            if (this.CropToSeasonMap.TryGetValue(sansPrefix, out var mappedInfo))
            {
                fruitCrop = this.GetRandomChoice(seed, key, mappedInfo);
                return true;
            }

            var cropInfo = ItemRegistry.GetData(key);
            if (cropInfo == null)
            {
                fruitCrop = default;
                Logger.Instance.Monitor.Log($"No item information registered for {key}", LogLevel.Error);
                return false;
            }

            if (this.CropToSeasonMap.TryGetValue(cropInfo.InternalName, out var mappedNameInfo))
            {
                fruitCrop = this.GetRandomChoice(seed, key, mappedNameInfo);
                return true;
            }

            fruitCrop = key;
            return true;
        }

        private static Dictionary<LewisSeasons, List<string>> CreateSeasonMap(Dictionary<string, SeasonStatusPair> source)
        {
            var seasonMap = new Dictionary<LewisSeasons, List<string>>();

            foreach (var kvp in source)
            {
                if (!kvp.Value.IsFruit) continue;
                if (seasonMap.TryGetValue(kvp.Value.Season, out var existing))
                {
                    existing.Add(kvp.Key);
                    continue;
                }
                seasonMap[kvp.Value.Season] = new List<string> { kvp.Key };
            }

            return seasonMap;
        }
    }
}
