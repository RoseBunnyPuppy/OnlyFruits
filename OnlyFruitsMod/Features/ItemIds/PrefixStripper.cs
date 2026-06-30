using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OnlyFruitsMod.Features.ItemIds
{
    public class PrefixStripper
    {
        private static Regex RexItemId { get; } = new Regex("^(\\(.*\\))(.*)$", RegexOptions.Compiled);

        public static PrefixStripper Instance { get; } = new PrefixStripper();

        /// <summary>
        ///   Attempt to extract the 'item prefix' and 'item id' from a full item id.
        /// </summary>
        public bool TrySplitFullItemId(
            string fullItemId, 
            [NotNullWhen(returnValue: true)] out string? prefix,
            [NotNullWhen(returnValue: true)] out string? itemId
        )
        {
            var match = RexItemId.Match(fullItemId);
            if (!match.Success)
            {
                prefix = default;
                itemId= default;
                return false;
            }
            prefix = match.Groups[1].Value;
            itemId = match.Groups[2].Value;
            return true;
        }
        public bool TryStripPrefix(string value, string prefix, out string cleaned)
        {
            if (!value.StartsWith(prefix))
            {
                cleaned = string.Empty;
                return false;
            }
            cleaned = value[prefix.Length..];
            return true;
        }
    }
}
