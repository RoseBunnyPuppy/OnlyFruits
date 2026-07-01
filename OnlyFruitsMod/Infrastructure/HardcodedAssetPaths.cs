using StardewValley.GameData;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Pants;
using StardewValley.GameData.Shirts;
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
        public static AssetDefinition<Dictionary<string, string>> DataBoots { get; } = "Data/Boots";
        public static AssetDefinition<Dictionary<string, string>> DataFurniture { get; } = "Data/Furniture";
        public static AssetDefinition<Dictionary<string, string>> DataHats { get; } = "Data/Hats";
        public static AssetDefinition<Dictionary<string, MannequinData>> DataMannequins { get; } = "Data/Mannequins";
        public static AssetDefinition<Dictionary<string, PantsData>> DataPants { get; } = "Data/Pants";
        public static AssetDefinition<Dictionary<string, ShirtData>> DataShirts { get; } = "Data/Shirts";
        public static AssetDefinition<Dictionary<string, ToolData>> DataTools{ get; } = "Data/Tools";
        public static AssetDefinition<Dictionary<string, TrinketData>> DataTrinkets { get; } = "Data/Trinkets";
        public static AssetDefinition<List<ModWallpaperOrFlooring>> DataAdditionalWallpaperFlooring { get; } = "Data/AdditionalWallpaperFlooring";
        public static AssetDefinition<Dictionary<string, WeaponData>> DataWeapons { get; } = "Data/Weapons";
        public static AssetDefinition<Dictionary<string, BigCraftableData>> DataBigCraftables { get; } = "Data/BigCraftables";
        public static AssetDefinition<Dictionary<string, SpecialOrderData>> DataSpecialOrders { get; } = "Data/SpecialOrders";
        public static AssetDefinition<Dictionary<string, string>> SpecialOrderStrings { get; } = "Strings/SpecialOrderStrings";
        public static AssetDefinition<Dictionary<string, MonsterSlayerQuestData>> DataMonsterSlayerQuests { get; } = "Data/MonsterSlayerQuests";
        public static AssetDefinition<Dictionary<string, string>>  DataQuests { get; } = "Data/Quests";
        public static AssetDefinition<Dictionary<string, ShopData>> DataShops { get; } = "Data/Shops";
    }
}
