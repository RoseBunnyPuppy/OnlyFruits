using StardewValley.GameData;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Shops;
using StardewValley.GameData.SpecialOrders;
using StardewValley.GameData.Tools;
using StardewValley.GameData.Weapons;

namespace OnlyFruitsMod.Infrastructure
{
    public static class HardcodedAssetPaths
    {
        public static AssetDefinition<Dictionary<string, string>> CookingRecipes { get; } = new("Data/CookingRecipes");
        public static AssetDefinition<Dictionary<string, string>> CraftingRecipes { get; } = new("Data/CraftingRecipes");
        public static AssetDefinition<Dictionary<string, ObjectData>> DataObjects { get; } = "Data/Objects";
        public static AssetDefinition<Dictionary<string, SpecialOrderData>> DataSpecialOrders { get; } = "Data/SpecialOrders";
        public static AssetDefinition<Dictionary<string, string>> SpecialOrderStrings { get; } = "Strings/SpecialOrderStrings";
        public static AssetDefinition<Dictionary<string, MonsterSlayerQuestData>> DataMonsterSlayerQuests { get; } = "Data/MonsterSlayerQuests";
        public static AssetDefinition<Dictionary<string, string>>  DataQuests { get; } = "Data/Quests";
        public static AssetDefinition<Dictionary<string, ShopData>> DataShops { get; } = "Data/Shops";
    }
}
