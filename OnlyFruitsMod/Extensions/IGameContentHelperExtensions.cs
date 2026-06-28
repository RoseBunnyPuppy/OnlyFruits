using OnlyFruitsMod.Infrastructure;
using StardewModdingAPI;

namespace OnlyFruitsMod.Extensions
{
    public static class IGameContentHelperExtensions
    {
        /// <summary>
        ///  Load content from the game folder or mod folder (if not already cached), and return it. When loading a <c>.png</c> file, this must be called outside the game's draw loop.
        /// </summary>
        public static TAsset LoadAsset<TAsset>(this IGameContentHelper contentHelper, AssetDefinition<TAsset> asset)
            where TAsset : notnull
        {
            return contentHelper.Load<TAsset>(asset.Name);
        }

        /// <summary>Get a helper to manipulate the data as a dictionary.</summary>
        /// <typeparam name="TKey">The expected dictionary key.</typeparam>
        /// <typeparam name="TValue">The expected dictionary value.</typeparam>
        /// <exception cref="InvalidOperationException">The content being read isn't a dictionary.</exception>
        public static IAssetDataForDictionary<TKey, TValue> AsAutoDictionary<TKey, TValue>(this IAssetData assetData, AssetDefinition<Dictionary<TKey, TValue>> _)
            where TKey : notnull
        {
            return assetData.AsDictionary<TKey, TValue>();
        }
    }
}
