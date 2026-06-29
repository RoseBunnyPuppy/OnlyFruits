using OnlyFruitsMod.Models;

namespace OnlyFruitsMod
{
    public static class DefaultItemIdConfigProvider
    {
        public static ItemIdConfigModel GetDefaults()
        {
            return new ItemIdConfigModel
            {
                ArtisinalItemIds = new HashSet<string>
                {
                    // Jelly_Name
                    "344",
                    // Wine_Name
                    "348",
                    // Raisins
                    "Raisins",
                    // Dried Fruit
                    "DriedFruit",
                },
                MemeItemIds = new()
                {
                    // CherryBomb_Name
                    "286",
                },
                ShouldBeFruitItemIds = new()
                {
                    // Sweet Gem Berry
                    "417",
                },
                ExplicitlyExcluded = new HashSet<string>
                {
                    // Summer Seeds
                    "496",
                    // Fall Seeds
                    "497",
                    // Winter Seeds
                    "498",
                    // Warp Totem Desert
                    "261",
                },
            };
        }
    }
}
