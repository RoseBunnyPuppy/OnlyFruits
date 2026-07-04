using OnlyFruitsMod.Features.Logging;
using StardewModdingAPI;
using System.Reflection;

namespace OnlyFruitsMod.Features.UpdateHelpers
{
    public record UpdateVersionInfo(Version Version, string Url);

    public class ModUpdateHelper
    {
        private readonly IModHelper helper;
        const string IModInfo_UpdateCheckDataProperty = "UpdateCheckData";
        public ModUpdateHelper(
            IModHelper helper
        )
        {
            this.helper = helper;
            
        }

        private Dictionary<Type, PropertyInfo?> CachedModInfoUpdateCheckProperties = new();
        private Dictionary<Type, PropertyInfo?> UpdateCheckDataProperties = new();

        private PropertyInfo? GetCachedPropertyInfo(Dictionary<Type, PropertyInfo?> lookups, object instance, string name)
        {

            var infoType = instance.GetType();
            if (!lookups.TryGetValue(infoType, out var propInfo))
            {
                propInfo = infoType.GetProperty(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                lookups[infoType] = propInfo;
            }
            return propInfo;
        }


        
        public UpdateVersionInfo? GetUpdateInformation(string modId)
        {
            const string UpdateCheckDataProperty = "UpdateCheckData";
            const string ExpectedCheckDataType = "ModEntryModel";
            const string SuggestedUpdateProperty = "SuggestedUpdate";

            var modInfo = this.helper.ModRegistry.Get(modId);
            if (modInfo == null)
            {
                //Logger.Instance.Log($"Failed to get mod info for own mod.", LogLevel.Error);
                return null;
            }

            var updateCheckPropInfo = this.GetCachedPropertyInfo(this.CachedModInfoUpdateCheckProperties, modInfo, UpdateCheckDataProperty);
            if (updateCheckPropInfo == null) return null;

            var checkData = updateCheckPropInfo.GetValue(modInfo);
            var actualType = checkData?.GetType().Name;
            if (checkData == null || actualType != ExpectedCheckDataType)
            {
                Logger.Instance.Log($"Unexpected '{UpdateCheckDataProperty}' type: '{actualType ?? "[null]"}", LogLevel.Error);
                return null;
            }


            var suggestedUpdatePropInfo = this.GetCachedPropertyInfo(this.UpdateCheckDataProperties, checkData, SuggestedUpdateProperty);
            if (suggestedUpdatePropInfo == null) return null;
            // +		_version	{0.0.7}	object {StardewModdingAPI.Toolkit.SemanticVersion}

            //SuggestedUpdate	null	StardewModdingAPI.Toolkit.Framework.Clients.WebApi.ModEntryVersionModel
            
            var value = suggestedUpdatePropInfo.GetValue(checkData);
            if (value == null) return null; 

            var _version = value.GetType().GetProperty("Version", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(value);
            var _url = value.GetType().GetProperty("Url", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(value)?.ToString();
            var versionString = _version?.ToString();
            if (_url == null || versionString == null)return null;
            return new(new Version(versionString), _url);
        }
        public bool DoesHaveSuggestedUpdates(IModInfo? modInfo)
        {
            if (modInfo == null) return false;

            var infoType = modInfo.GetType();
            if (!this.CachedModInfoUpdateCheckProperties.TryGetValue(infoType, out var propInfo))
            {
                propInfo = infoType.GetProperty("UpdateCheckData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                this.CachedModInfoUpdateCheckProperties[infoType] = propInfo;
            }

            if (propInfo == null) return false;

            var checkData = propInfo.GetValue(modInfo);
            _ = 23;
            return false;
        }

        
    }
}
