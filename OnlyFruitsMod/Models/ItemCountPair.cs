namespace OnlyFruitsMod.Models
{
    public class ItemCountPair
    {
        public string ItemId { get; set; }
        public string? Count { get; set; }

        public ItemCountPair(
            string itemId,
            string? count = null
        ) {
            this.ItemId = itemId;
            this.Count = count;
        }

        public override string ToString()
        {
            if (this.Count == null) return this.ItemId;
            return $"{this.ItemId} {this.Count}";
        }
    }

}
