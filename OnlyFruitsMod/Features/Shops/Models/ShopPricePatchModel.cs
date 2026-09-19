using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyFruitsMod.Features.Shops.Models
{
    public class ShopOverrideModel
    {
        public const int MagicFallback = -2;
        public const int MagicBypass = -3;
        /// <summary>
        ///   If set, the price to assign an item that is NOT specified by the <see cref="ItemPrices"/>.
        /// </summary>
        public int? FallbackPrice { get; set; }

        /// <summary>
        ///   If set, a mapping of "item id" to "price".
        /// </summary>
        public Dictionary<string, int>? ItemPrices { get; set; }
    }
    public class ShopPricePatchModel
    {
        public const string CommentShopPrefix = "## ";
        public void Clean()
        {
            this.UnmodifiedShops?.RemoveWhere(x => x.StartsWith(CommentShopPrefix));
        }
        public static ShopPricePatchModel CreateDefault()
        {
            return new ShopPricePatchModel
            {
                UnmodifiedShops = new(){
                    // catalogues allow all shit to be purchased for free
                    "Catalogue",
                    "Furniture Catalogue",
                    "JojaFurnitureCatalogue",
                    "JunimoFurnitureCatalogue",
                    "RetroFurnitureCatalogue",
                    "TrashFurnitureCatalogue",
                    "WizardFurnitureCatalogue",

                    // nothing here should cost gold
                    "DesertTrade",
                    // nothing here should cost gold
                    "BooksellerTrade",

                    // desert festival villager shops
                    "DesertFestival_Abigail",
                    "DesertFestival_Alex",
                    "DesertFestival_Caroline",
                    "DesertFestival_Clint",
                    "DesertFestival_Demetrius",
                    "DesertFestival_Elliott",
                    "DesertFestival_Emily",
                    "DesertFestival_Evelyn",
                    "DesertFestival_George",
                    "DesertFestival_Gus",
                    "DesertFestival_Haley",
                    "DesertFestival_Harvey",
                    "DesertFestival_Jas",
                    "DesertFestival_Jodi",
                    "DesertFestival_Kent",
                    "DesertFestival_Leah",
                    "DesertFestival_Leo",
                    "DesertFestival_Marnie",
                    "DesertFestival_Maru",
                    "DesertFestival_Pam",
                    "DesertFestival_Penny",
                    "DesertFestival_Pierre",
                    "DesertFestival_Robin",
                    "DesertFestival_Sam",
                    "DesertFestival_Sebastian",
                    "DesertFestival_Shane",
                    "DesertFestival_Vincent",

                    // calico egg merchant
                    "DesertFestival_EggShop",
                    // other non-gold shops
                    "QiGemShop",
                    "IslandTrade",
                    "Raccoon",
                },
                PriceOverrides = new()
                {
                    ["VolcanoShop"] = new()
                    {
                        ItemPrices = new()
                        {
                            ["(O)Book_Diamonds"] = 0,
                            ["(B)853"] = 0,
                        },
                    },
                    ["LostItems"] = new()
                    {
                        FallbackPrice = 10_000,
                    },
                }
            };
        }

        /// <summary>
        ///   The shop ids for which we will not make any changes.  If a shop id
        ///   starts with <see cref="CommentShopPrefix"/>, it is treated as a 
        ///   comment/groupin header within the json, and can be removed by
        ///   calling <see cref="Clean"/>.
        /// </summary>
        public HashSet<string>? UnmodifiedShops { get; set; }

        /// <summary>
        ///   A mapping from 'shop id' to its item price map.
        /// </summary>
        public Dictionary<string, ShopOverrideModel>? PriceOverrides { get; set; }


        /// <summary>
        ///     Attempt to get the <see cref="ShopOverrideModel"/> for the given
        ///   <paramref name="shopId"/>
        /// </summary>
        public bool TryGetShopPriceOverrides(
            string shopId,
            [NotNullWhen(returnValue: true)] 
            out ShopOverrideModel? shopOverride
        )
        {
            var overrides = this.PriceOverrides;
            if (overrides == null)
            {
                shopOverride = default;
                return false;
            }
            return overrides.TryGetValue(shopId, out shopOverride);
        }
    }
}
