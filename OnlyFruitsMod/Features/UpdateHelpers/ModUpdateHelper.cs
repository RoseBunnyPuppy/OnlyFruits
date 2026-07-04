using OnlyFruitsMod.Features.Logging;
using StardewModdingAPI;
using System.Reflection;

namespace OnlyFruitsMod.Features.UpdateHelpers
{
    public record UpdateVersionInfo(Version Version, string Url);

    public class ModUpdateHelper
    {
        private readonly IModHelper helper;

        private Dictionary<string, UpdateVersionInfo?> CachedUpdateInfo { get; }

        public ModUpdateHelper(
            IModHelper helper
        )
        {
            this.helper = helper;
            this.CachedUpdateInfo = new Dictionary<string, UpdateVersionInfo?>();
        }

        public void ResetUpdateInfo()
        {
            this.CachedUpdateInfo.Clear();
        }
        
        public UpdateVersionInfo? GetUpdateInformation(string modId, bool force = false)
        {
            if (force || !this.CachedUpdateInfo.TryGetValue(modId, out var updateVersionInfo))
            {
                this.CachedUpdateInfo[modId] = updateVersionInfo = this.ForceGetUpdateInformation(modId);
                return updateVersionInfo;
            }
            return updateVersionInfo;
        }

        private bool ExpectType(Type actual, string expected)
        {
            if (actual.Name == expected) return true;
            this.LogUnexpectedTypes(expected, actual: actual.Name);
            return false;
        }
        private void LogUnexpectedTypes(string expected, string actual)
        {
            Logger.Instance.LogOnce($"[{nameof(ModUpdateHelper)}] Unexpected internal type '{actual}', expected '{expected}'", LogLevel.Error);
        }

        private bool TryGetReflectedValue(object instance, string propertyName, string expectedType, out object? value)
        {
            var instanceType = instance.GetType();
            if (!this.ExpectType(actual: instanceType, expected: expectedType))
            {
                value = default;
                return false;
            }

            var propInfo = instanceType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (propInfo == null)
            {
                value = default;
                return false;
            }

            value = propInfo.GetValue(instance);
            return true;

        }
        private UpdateVersionInfo? ForceGetUpdateInformation(string modId)
        {
            
            const string UpdateCheckDataProperty = "UpdateCheckData";
            const string ExpectedCheckDataType = "ModEntryModel";
            const string SuggestedUpdateProperty = "SuggestedUpdate";
            const string ExpectedModInfoType = "ModMetadata";

            var modInfo = this.helper.ModRegistry.Get(modId);
            if (modInfo == null)
            {
                Logger.Instance.LogOnce($"[{nameof(ModUpdateHelper)}] Unknown mod '{modId}'", LogLevel.Error);
                return default;
            }

            // try to get the 'modInfo.UpdateCheckData' property
            if (!this.TryGetReflectedValue(modInfo, propertyName: UpdateCheckDataProperty, expectedType: ExpectedModInfoType, out var updateCheckDataValue)) return default;
            // fail if null
            if (updateCheckDataValue == null)
            {
                Logger.Instance.LogOnce($"[{nameof(ModUpdateHelper)}] Failed to get modInfo.{UpdateCheckDataProperty}: value was null", LogLevel.Error);
                return default;
            }

            // try to get the 'modInfo.UpdateCheckData.SuggestedUpdate' property
            if (!this.TryGetReflectedValue(updateCheckDataValue, propertyName: SuggestedUpdateProperty, expectedType: ExpectedCheckDataType, out var suggestedUpdateValue)) return default;

            if (suggestedUpdateValue == null) return null; 

            var _version = suggestedUpdateValue.GetType().GetProperty("Version", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(suggestedUpdateValue)?.ToString();
            var _url = suggestedUpdateValue.GetType().GetProperty("Url", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(suggestedUpdateValue)?.ToString();
            if (_url == null || _version == null) return null;
            return new(new Version(_version), _url);
        }
        
    }
}
