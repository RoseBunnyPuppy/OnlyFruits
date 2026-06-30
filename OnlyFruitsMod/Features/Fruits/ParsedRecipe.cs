using OnlyFruitsMod.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyFruitsMod.Features.Fruits
{

    /// <summary>
    ///   Represents an ingredient or result for a recipe.
    /// </summary>
    /// <param name="ItemId">A partial id for an <see cref="ItemIdPrefixes.Objects"/> item</param>
    /// <param name="Count"></param>
    [DebuggerDisplay("{ItemId,nq} {Count == null ? \"\" : Count,nq} ")]
    public record ItemCountPair(string ItemId, string? Count = null);

    /// <summary>
    ///   Contains the non-string-form data for a game recipe.
    /// </summary>
    public class ParsedRecipe
    {
        /// <summary>
        ///   The raw data.
        /// </summary>
        private string[] Parts { get; set; }

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

        /// <summary>
        ///     Split an ingredient array string into 
        ///   one or more id/count pairs.
        /// </summary>
        private static bool TryExtractIngredients(
            string value,
            [NotNullWhen(returnValue: true)] out IEnumerable<ItemCountPair>? ingredients
        )
        {
            ingredients = default;
            var parts = value.Trim().Split();
            if (parts.Length % 2 != 0) return false;

            ingredients = Enumerable.Range(0, parts.Length / 2)
                .Select(idx => new ItemCountPair(parts[idx * 2], parts[idx * 2 + 1]))
                .ToArray();
            return true;
        }

        private static bool TryParseItemCountPair(
            string value,
            [NotNullWhen(returnValue: true)] out ItemCountPair? parsed
        )
        {
            var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
            {
                parsed = new ItemCountPair(parts[0]);
                return true;
            }
            else if (parts.Length > 2)
            {
                parsed = default;
                return false;
            }
            parsed =new ItemCountPair(parts[0], parts[1]);
            return true;
        }



        public static bool TryParse(
            string value,
            [NotNullWhen(returnValue: true)] out ParsedRecipe? recipe
        )
        {
            recipe = default;
            var parts = value.Split('/');
            if (!TryExtractIngredients(parts[0], out var ingredients)) return false;
            if (!TryParseItemCountPair(parts[2], out var result)) return false;
            recipe = new ParsedRecipe(parts, ingredients, result);
            return true;
        }
    }

}
