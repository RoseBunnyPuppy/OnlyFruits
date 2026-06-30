using OnlyFruitsMod.Features.ItemIds;
using OnlyFruitsMod.Infrastructure;
using StardewValley;
using StardewValley.GameData.Objects;

namespace OnlyFruitsMod.Features.Prices
{
    
    public class PriceLocator
    {
        public static PriceLocator Instance { get; } = new PriceLocator();

        public bool TryGetItemPrice(string scope, string itemId, out int value) => this.TryGetFullIdPrice($"{scope}{itemId}", out value);
       
        public bool TryGetFullIdPrice(string itemId, out int value)
        {
            value = default;
            var item = ItemRegistry.GetData(itemId);
            if (item == null) return false;
            if (item.RawData is not ObjectData objData) return false;
            value = objData.Price;
            return true;
        }
        
    }
}
