namespace OnlyFruitsMod.Features.Fruits
{
    public static class DefaultItemIdConfigProvider
    {
        public static ItemIdConfigModel GetDefaults()
        {
            return new ItemIdConfigModel
            {
                ArtisinalFullItemIds = new HashSet<string>
                {
                    // Jelly_Name
                    "(O)344",
                    // Wine_Name
                    "(O)348",
                    // Raisins
                    "(O)Raisins",
                    // Dried Fruit
                    "(O)DriedFruit",
                },
                MemeFullItemIds = new()
                {
                    // CherryBomb_Name
                    "(O)286",
                },
                ShouldBeFruitFullItemIds = new()
                {
                    // Sweet Gem Berry
                    "(O)417",
                },
                ExplicitlyExcludedFullItemIds = new()
                {
                    // Summer Seeds
                    "(O)496",
                    // Fall Seeds
                    "(O)497",
                    // Winter Seeds
                    "(O)498",
                    // Warp Totem Desert
                    "(O)261",
                },
            };
        }
    }
}
