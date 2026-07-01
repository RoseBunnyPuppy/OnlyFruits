using StardewValley.GameData.SpecialOrders;
using System.Diagnostics.CodeAnalysis;

namespace OnlyFruitsMod.Extensions
{
    public static class RandomizedElementsExtensions
    {
        public static RandomizedElement? GetByName(this List<RandomizedElement> randomizedElements, string name)
        {
            return randomizedElements.FirstOrDefault(x => x.Name == name);
        }

        public static bool TryGetSingleByRequiredTag(
            this List<RandomizedElementItem> randomizedElements, 
            string expectedTag, 
            [NotNullWhen(returnValue: true)] out RandomizedElementItem? match
        )
        {
            match = randomizedElements.FirstOrDefault(x => x.RequiredTags == expectedTag);
            return match != null;
        }
    }
}
