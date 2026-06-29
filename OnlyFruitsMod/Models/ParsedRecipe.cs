using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyFruitsMod.Models
{
    [DebuggerDisplay("{ItemId,nq} {Count == null ? \"\" : Count,nq} ")]
    public record ItemCountPair(string ItemId, string? Count = null);

    public class ParsedRecipe
    {
        public string[] Parts { get; set; }
        public List<ItemCountPair> Ingredients { get; set; }
        public ItemCountPair Result { get; set; }

        public ParsedRecipe(
            string[] parts,
            IEnumerable<ItemCountPair> ingredients,
            ItemCountPair result
        )
        {
            this.Result = result;
            this.Parts = parts;
            this.Ingredients = ingredients.ToList();
        }

        private static List<ItemCountPair> ExtractIngredients(string value)
        {
            var parts = value.Trim().Split();
            if (parts.Length % 2 != 0)
            {
                Debugger.Break();
                throw new NotImplementedException();
            }
            return Enumerable.Range(0, parts.Length / 2).Select(idx => new ItemCountPair(parts[idx * 2], parts[idx * 2 + 1])).ToList();
        }

        private static ItemCountPair ParseItemCountPair(string value)
        {
            var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return new ItemCountPair(parts[0]);
            else if (parts.Length > 2) throw new ArgumentException($"Expected at most 2 parts, found {parts.Length}", nameof(value));
            return new ItemCountPair(parts[0], parts[1]);
        }
        public static ParsedRecipe Parse(string value)
        {
            var parts = value.Split('/');
            var ingredients = ExtractIngredients(parts[0]);
            return new ParsedRecipe(parts, ingredients, ParseItemCountPair(parts[2]));
        }
    }

}
