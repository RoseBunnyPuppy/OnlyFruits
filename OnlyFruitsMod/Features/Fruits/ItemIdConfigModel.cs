namespace OnlyFruitsMod.Features.Fruits
{
    public class ItemIdConfigModel
    {
        /// <summary>
        ///   Non-fruit items we are allowing.
        /// </summary>
        public HashSet<string> MemeFullItemIds { get; set; } = new();

        /// <summary>
        ///   Non-fruit items that ConcernedApe should've treated as fruit.
        /// </summary>
        public HashSet<string> ShouldBeFruitFullItemIds { get; set; } = new();

        /// <summary>
        ///   The 'full' ids of items to exclude, regardless of whether they are a fruit or derived fruit.
        /// </summary>
        public HashSet<string> ExplicitlyExcludedFullItemIds { get; set; } = new();

        /// <summary>
        ///   The 'full' ids of artisanal items to allow.
        /// </summary>
        public HashSet<string> ArtisanalFullItemIds { get; set; } = new();
    }

}
