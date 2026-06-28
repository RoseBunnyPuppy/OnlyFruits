using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyFruitsMod.Features.ItemIds
{
    public class PrefixStripper
    {
        public static PrefixStripper Instance { get; } = new PrefixStripper();
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
