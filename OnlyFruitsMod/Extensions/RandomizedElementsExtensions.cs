using StardewValley.GameData.SpecialOrders;

namespace OnlyFruitsMod.Extensions
{
    public static class RandomizedElementsExtensions
    {
        public static RandomizedElement? GetByName(this List<RandomizedElement> randomizedElements, string name)
        {
            return randomizedElements.FirstOrDefault(x => x.Name == name);
        }
        public static RandomizedElementItem ExpectSingleByRequiredTag(this List<RandomizedElementItem> randomizedElements, string expectedTag)
        {
            var match = randomizedElements.FirstOrDefault(x => x.RequiredTags == expectedTag);
            return match ?? throw new InvalidOperationException($"Failed to find single item with tag '{expectedTag}'");
        }
    }
}
