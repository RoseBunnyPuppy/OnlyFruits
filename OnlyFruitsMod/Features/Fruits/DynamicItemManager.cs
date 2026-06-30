using OnlyFruitsMod.Features.ModConfiguration;
using OnlyFruitsMod.Infrastructure;
using OnlyFruitsMod.Models;
using StardewModdingAPI;
using StardewValley.GameData.Objects;

namespace OnlyFruitsMod.Features.Fruits
{
    public enum InclusionReasons
    {
        Fruit = 0x10,
        ShouldBeFruit = 0x15,
        Meme = 0x30,
        Artisnal = 0x80,
        Derived = 0x90,
    }
    public record IdReasonPair(string Id, InclusionReasons Reason);
    public class DynamicItemManager
    {
        public bool Verbose { get; set; } = false;
        private readonly IMonitor monitor;
        private readonly ModConfigInstance configInstance;
        public RecipeTracker CookingRecipes { get; } = new();
        public RecipeTracker CraftingRecipes { get; } = new();

        private ModConfig Config => this.configInstance.Config;

        public ItemIdConfigModel IdConfigModel { get; set; }

        private record ReasonPairCollection(HashSet<string> UniqueIds, List<IdReasonPair> ReasonedItems);
        private record ScopedIdReasonPair(string Scope, IdReasonPair ReasonPair);
        Dictionary<string, ReasonPairCollection> ScopedUniqueItemIds { get; } = new();
        List<ScopedIdReasonPair> AllItems { get; } = new();

        public DynamicItemManager(
            ItemIdConfigModel idConfigModel,
            IMonitor monitor,
            ModConfigInstance configInstance
        )
        {
            this.IdConfigModel = idConfigModel;
            this.monitor = monitor;
            this.configInstance = configInstance;
        }

        /// <summary>
        ///   Tests if the <paramref name="itemId"/> is a fruity id.
        /// </summary>
        public bool IsFruityId(string scope, string itemId)
        {
            if (!this.ScopedUniqueItemIds.TryGetValue(scope, out var scopedItems)) return false;
            return scopedItems.UniqueIds.Contains(itemId);
        }

        public int Count => this.AllItems.Count;

        public bool Add(string scope, string itemId, InclusionReasons reason)
        {
            if (scope == ItemIdPrefixes.Objects)
            {
                // dont add anything we've explicitly excluded
                if (this.IdConfigModel.ExplicitlyExcluded.Contains(itemId)) return false;
            }
            // do nothing if already included
            if (!this.ScopedUniqueItemIds.TryGetValue(scope, out var scopedItems))
            {
                this.ScopedUniqueItemIds[scope] = scopedItems = new(new(), new());
            }
            if (!scopedItems.UniqueIds.Add(itemId)) return false;
            IdReasonPair pair = new(itemId, reason);
            scopedItems.ReasonedItems.Add(pair);
            this.AllItems.Add(new(scope, pair));
            return true;
        }


        public void Clear()
        {
            this.AllItems.Clear();
            this.ScopedUniqueItemIds.Clear();
        }

        private IEnumerable<string> GetScopedItemIds(string scope)
        {
            if (!this.ScopedUniqueItemIds.TryGetValue(scope, out var collection)) return Array.Empty<string>();
            return collection.UniqueIds;
        }
        public void ApplyAssetData(IDictionary<string, ObjectData> data)
        {
            string[] RunBatch(string label, Action action)
            {
                string[] newItems = Array.Empty<string>();

                var preItems = this.GetScopedItemIds(ItemIdPrefixes.Objects).ToArray();
                var preCount = this.Count;
                if(this.Verbose) this.monitor.Log($"Finding '{label}'.", LogLevel.Debug);
                action();
                var actualCount = this.Count - preCount;
                if (actualCount > 0)
                    newItems = this.GetScopedItemIds(ItemIdPrefixes.Objects).Except(preItems).ToArray();
                if (this.Verbose) this.monitor.Log($"Found {actualCount} '{label}'.", LogLevel.Debug);
                return newItems;
            }
            bool RunRecipeBatch(string label, RecipeTracker recipeTracker)
            {
                var preCount = this.Count;
                var preItems = this.GetScopedItemIds(ItemIdPrefixes.Objects).ToArray();
                var allowedCategory = StardewValley.Object.FruitsCategory.ToString();
                foreach (var recipe in recipeTracker.Recipes)
                {
                    var hasFruitCategory = recipe.Ingredients.Any(pair => pair.ItemId == allowedCategory);
                    var hasFruitItem = recipe.Ingredients.Any(pair => this.IsFruityId(ItemIdPrefixes.Objects, pair.ItemId));

                    if (!(hasFruitCategory || hasFruitItem)) continue;

                    this.Add(ItemIdPrefixes.Objects, recipe.Result.ItemId, InclusionReasons.Derived);
                }
                var addedItems = this.GetScopedItemIds(ItemIdPrefixes.Objects).Except(preItems).ToArray();
                if (addedItems.Any())
                {
                    if (this.Verbose) this.monitor.Log($"================", LogLevel.Debug);
                    foreach (var addedId in addedItems)
                    {
                        if (this.Verbose) this.monitor.Log($"{label}: {addedId} '{data[addedId].DisplayName}'", LogLevel.Debug);
                    }
                }
                var postCount = this.Count;
                return postCount != preCount;
            }

            void FindAllDerivedCookingRecipes()
            {
                while (RunRecipeBatch("cooking", this.CookingRecipes))
                {
                    _ = 23;
                }
            }
            void FindAllDerivedCraftingRecipes()
            {
                while (RunRecipeBatch("crafting", this.CraftingRecipes))
                {
                    _ = 23;
                }
            }

            // find items within the 'fruit' category
            RunBatch("Fruit Category Items", () =>
            {
                foreach ((string itemID, ObjectData itemData) in data)
                {
                    // skip fruits
                    if (itemData.Category == StardewValley.Object.FruitsCategory)
                    {
                        this.Add(ItemIdPrefixes.Objects, itemID, InclusionReasons.Fruit);
                        continue;
                    }
                }
            });
            // add the artisinal items
            RunBatch("Artisinal items", () =>
            {
                if (!this.Config.AllowSellingArtisinalItems) return;

                foreach ((string itemID, ObjectData itemData) in data)
                {
                    // skip fruits
                    if (this.IdConfigModel.ArtisinalItemIds.Contains(itemID))
                    {
                        this.Add(ItemIdPrefixes.Objects, itemID, InclusionReasons.Artisnal);
                        continue;
                    }
                }
            });

            RunBatch("Should've Been Fruits", () =>
            {
                if (!this.Config.AllowSellingShouldaBeenFruitItems) return;
                foreach ((string itemID, ObjectData itemData) in data)
                {
                    // skip fruits
                    if (this.IdConfigModel.ShouldBeFruitItemIds.Contains(itemID))
                    {
                        this.Add(ItemIdPrefixes.Objects, itemID, InclusionReasons.ShouldBeFruit);
                        continue;
                    }
                }
            });


            // meme items
            RunBatch("Meme Items", () =>
            {
                if (!this.Config.AllowSellingMemeItems) return;

                foreach ((string itemID, ObjectData itemData) in data)
                {
                    // skip fruits
                    if (this.IdConfigModel.MemeItemIds.Contains(itemID))
                    {
                        this.Add(ItemIdPrefixes.Objects, itemID, InclusionReasons.Meme);
                        continue;
                    }
                }
            });


            // cooking recipes that utilize a fruit
            RunBatch("Derived cooking items", () =>
            {
                if (!this.Config.AllowSellingAutoDerivedItems) return;
                FindAllDerivedCookingRecipes();
            });
            // crafting recipes that utilize a fruit
            RunBatch("Derived crafting items", () =>
            {
                if (!this.Config.AllowSellingAutoDerivedItems) return;
                FindAllDerivedCraftingRecipes();
            });
        }

    }
}
