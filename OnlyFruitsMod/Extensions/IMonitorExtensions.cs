using StardewModdingAPI;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace OnlyFruitsMod.Extensions
{
    public static class IMonitorExtensions
    {
        public static void LogAssetReady(this IMonitor monitor, object source, string? assetName)
        {
#if !DisableDevHelpers
            monitor.LogDebug($"[{source.GetType().Name}] {assetName} (AssetReady)");
#endif
        }
        public static void LogAssetReady(this IMonitor monitor, object source, IAssetName? assetName) =>
            monitor.LogAssetReady(source, assetName?.ToString());


        public static void LogAssetRequested(this IMonitor monitor, object source, string? assetName)
        {
#if !DisableDevHelpers
            monitor.LogDebug($"[{source.GetType().Name}] {assetName} (AssetRequested)");
#endif
        }
        public static void LogAssetRequested(this IMonitor monitor, object source, IAssetName? assetName) =>
            monitor.LogAssetRequested(source, assetName?.ToString());


        public static void LogAssetInvalidated(this IMonitor monitor, object source, string? assetName)
        {
#if !DisableDevHelpers
            monitor.LogDebug($"[{source.GetType().Name}] {assetName} (AssetInvalidated)");
#endif
        }
        public static void LogAssetInvalidated(this IMonitor monitor, object source, IAssetName? assetName) =>
            monitor.LogAssetInvalidated(source, assetName?.ToString());


        public static void LogDebug(this IMonitor monitor, string content, LogLevel? level = default)
        {
            monitor.Log(content, LogLevel.Debug);
#if !DisableDevHelpers
            Debug.WriteLine(content);
#endif
        }
    }
}
