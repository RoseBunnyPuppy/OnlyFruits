using OnlyFruitsMod.Models;
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

        public void LoadFromAssetEvent(AssetRequestedEventArgs e)
        {
            e.Edit(asset =>
            {
                this.Clear();
                var data = asset.AsDictionary<string, string>().Data;
                foreach ((string itemID, string itemData) in data)
                {
                    this.Lookups[itemID] = ParsedRecipe.Parse(itemData);
                }
            });
        }
    }
}
