using OnlyFruitsMod.Features.Logging;
using StardewModdingAPI;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace OnlyFruitsMod.Extensions
{
    public static class IMonitorExtensions
    {
        public static void LogAssetReady(this ILogger monitor, object source, string? assetName)
        {
#if !DisableDevHelpers
            monitor.Log($"[{source.GetType().Name}] {assetName} (AssetReady)", LogLevel.Trace);
#endif
        }
        public static void LogAssetReady(this ILogger monitor, object source, IAssetName? assetName) =>
            monitor.LogAssetReady(source, assetName?.ToString());


        public static void LogAssetRequested(this ILogger monitor, object source, string? assetName)
        {
#if !DisableDevHelpers
            monitor.Log($"[{source.GetType().Name}] {assetName} (AssetRequested)", LogLevel.Trace);
#endif
        }
        public static void LogAssetRequested(this ILogger monitor, object source, IAssetName? assetName) =>
            monitor.LogAssetRequested(source, assetName?.ToString());


        public static void LogAssetInvalidated(this ILogger monitor, object source, string? assetName)
        {
#if !DisableDevHelpers
            monitor.Log($"[{source.GetType().Name}] {assetName} (AssetInvalidated)", LogLevel.Trace);
#endif
        }
        public static void LogAssetInvalidated(this ILogger monitor, object source, IAssetName? assetName) =>
            monitor.LogAssetInvalidated(source, assetName?.ToString());


        public static void LogDebug(this ILogger logger, string content, LogLevel? level = default)
        {
            logger.Log(content, LogLevel.Debug);
#if !DisableDevHelpers
            Debug.WriteLine(content);
#endif
        }
    }
}
