namespace OnlyFruitsMod.Features.ItemIds
{
    /// <summary>
    ///   Represents an 'item scope' - 'partial id' pair.
    /// </summary>
    /// <param name="Scope">The type of item</param>
    /// <param name="PartialId">Id within the scope</param>
    public record ScopedItemId(string Scope, string PartialId)
    {
        public string FullId => $"{this.Scope}{this.PartialId}";
    }
}
