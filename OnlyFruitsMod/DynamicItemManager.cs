using OnlyFruitsMod.Features.Fruits;
using OnlyFruitsMod.Features.ModConfiguration;
using OnlyFruitsMod.Infrastructure;
using OnlyFruitsMod.Models;
using StardewModdingAPI;
using StardewValley.GameData.Objects;

namespace OnlyFruitsMod
{
    public record IdReasonPair(string id, string reason);
    public class DynamicItemManager
    {
        public bool Verbose { get; set; } = false;
        private readonly IMonitor monitor;
        private readonly ModConfigInstance configInstance;
        public RecipeTracker CookingRecipes { get; } = new();
        public RecipeTracker CraftingRecipes { get; } = new();

        private ModConfig Config => this.configInstance.Config;

        public ItemIdConfigModel IdConfigModel { get; set; }

        private HashSet<string> UniqueItemIds { get; } = new HashSet<string>();
        private List<IdReasonPair> ItemIdReasonPairs { get; } = new List<IdReasonPair>();

        public IEnumerable<string> ItemIds { get { return new List<string>(UniqueItemIds); } }
        public IEnumerable<IdReasonPair> FullItemIds { get { return new List<IdReasonPair>(this.ItemIdReasonPairs); } }

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
        public bool IsFruityId(string itemId) => this.UniqueItemIds.Contains(itemId);
        public int Count => this.UniqueItemIds.Count;

        public bool Add(string itemId, string reason)
        {
            if (this.IdConfigModel.ExplicitlyExcluded.Contains(itemId)) return false;
            if (!this.UniqueItemIds.Add(itemId)) return false;
            this.ItemIdReasonPairs.Add(new(itemId, reason));
            return true;
        }


        public void Clear()
        {
            this.UniqueItemIds.Clear();
            this.ItemIdReasonPairs.Clear();
        }

        public void ApplyAssetData(IDictionary<string, ObjectData> data)
        {
            string[] RunBatch(string label, Action action)
            {
                string[] newItems = Array.Empty<string>();

                var preItems = this.ItemIds.ToArray();
                var preCount = this.Count;
                if(this.Verbose) this.monitor.Log($"Finding '{label}'.", LogLevel.Debug);
                action();
                var actualCount = this.Count - preCount;
                if (actualCount > 0)
                    newItems = this.ItemIds.Except(preItems).ToArray();
                if (this.Verbose) this.monitor.Log($"Found {actualCount} '{label}'.", LogLevel.Debug);
                return newItems;
            }
            bool RunRecipeBatch(string label, RecipeTracker recipeTracker)
            {
                var preCount = this.Count;
                var preItems = this.ItemIds.ToArray();
                foreach (var recipe in recipeTracker.Recipes)
                {
                    var hasFruitCategory = recipe.Ingredients.Any(pair => pair.ItemId == HardcodedObjectIds.FruitsCategoryString);
                    var hasFruitItem = recipe.Ingredients.Any(pair => this.IsFruityId(pair.ItemId));

                    if (!(hasFruitCategory || hasFruitItem)) continue;

                    this.Add(recipe.Result.ItemId, HardcodedInclusionReasons.Derived);
                }
                var addedItems = this.ItemIds.Except(preItems).ToArray();
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
                        this.Add(itemID, HardcodedInclusionReasons.Fruit);
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
                        this.Add(itemID, HardcodedInclusionReasons.ForcedArtisinal);
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
                        this.Add(itemID, HardcodedInclusionReasons.ShouldBeFruit);
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
                        this.Add(itemID, HardcodedInclusionReasons.Meme);
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
