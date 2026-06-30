using StardewModdingAPI.Events;
using StardewValley.GameData.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyFruitsMod.Features.Fruits
{
    
    public class RecipeTracker
    {
        private Dictionary<string, ParsedRecipe> Lookups { get; } = new Dictionary<string, ParsedRecipe>();
        public IEnumerable<ParsedRecipe> Recipes => this.Lookups.Values;

        public void Clear()
        {
            this.Lookups.Clear();
        }

        public void LoadRecipeMap(IDictionary<string, string> recipeMap)
        {
                this.Clear();
                foreach (var (recipeId, rawRecipe) in recipeMap)
                {
                    // dont parse wonky recipes
                    if (!ParsedRecipe.TryParse(rawRecipe, out var recipe)) continue;
                    this.Lookups[recipeId] = recipe;
                }
        }
    }
}
