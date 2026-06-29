namespace OnlyFruitsMod.Models
{
    public class ItemIdConfigModel
    {
        /// <summary>
        ///   Non-fruit items we are allowing.
        /// </summary>
        public HashSet<string> MemeItemIds { get; set; } = new HashSet<string>();

        /// <summary>
        ///   Non-fruit items that ConcernedApe shoulda treated as fruit.
        /// </summary>
        public HashSet<string> ShouldBeFruitItemIds { get; set; } = new HashSet<string>();

        /// <summary>
        ///   Items we exclude, regardless of whether they are a fruit or derived fruit.
        /// </summary>
        public HashSet<string> ExplicitlyExcluded { get; set; } = new HashSet<string>();
        public HashSet<string> ArtisinalItemIds { get; set; } = new HashSet<string>();
    }

}
