using OnlyFruitsMod.Infrastructure;
using StardewValley;

namespace OnlyFruitsMod.Extensions
{

    public static class ItemCachedModDataPriceExtensions
    {
        /// <summary>
        ///     Attempt to set the original price within the 
        ///   item's modData.  If <paramref name="force"/> is
        ///   false, the value will only be set if it is
        ///   not already set.
        /// </summary>
        public static bool TrySetCachedModDataPrice(this Item item, int value, bool force = false)
        {
            // if there is no price stored in the mod data, update it
            if (force || !item.modData.TryGetValue(HardcodedModDataKeys.OriginalPriceModDataKey, out _))
            {
                item.modData[HardcodedModDataKeys.OriginalPriceModDataKey] = value.ToString();
                return true;
            }
            return false;
        }

        public static bool TryGetDirectlyCachedModDataPrice(this Item item, out int value)
        {
            value = default;
            // fail if not set
            if (!item.modData.TryGetValue(HardcodedModDataKeys.OriginalPriceModDataKey, out var currentValue)) return false;

            return int.TryParse(currentValue, out value);
        }
    }
}
