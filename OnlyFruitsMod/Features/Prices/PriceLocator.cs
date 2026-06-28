using OnlyFruitsMod.Features.ItemIds;
using OnlyFruitsMod.Infrastructure;
using StardewValley;
using StardewValley.GameData.Objects;

namespace OnlyFruitsMod.Features.Prices
{
    
    public class PriceLocator
    {
        public static PriceLocator Instance { get; } = new PriceLocator();
        
        public bool TryGetItemPrice(string itemId, out int value)
        {
            value = default;
            var item = ItemRegistry.GetData(itemId);
            if (item == null) return false;
            if (item.RawData is not ObjectData objData) return false;
            value = objData.Price;
            return true;
        }
        public bool TryGetCropPrice(string itemId, out int value)
        {
            if (this.TryGetItemPrice(itemId, out value)) return true;

            if (PrefixStripper.Instance.TryStripPrefix(itemId, ItemIdPrefixes.OPrefix, out var cleaned))
                return this.TryGetItemPrice(cleaned, out value);

            return false;
        }
    }
}
