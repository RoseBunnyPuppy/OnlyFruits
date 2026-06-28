using StardewValley.GameData.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyFruitsMod.Features.Prices
{
    public class PriceCache
    {
        public Dictionary<string, int> CachedPrices { get; set; } = new Dictionary<string, int>();

        public void Clear()
        {
            this.CachedPrices.Clear();
        }
        public void ResetPriceData(IDictionary<string, ObjectData> assetData)
        {
            this.Clear();

            foreach (var kvp in assetData)
            {
                // cache the original price
                this.CachedPrices[kvp.Key] = kvp.Value.Price;
            }
        }
    }
}
