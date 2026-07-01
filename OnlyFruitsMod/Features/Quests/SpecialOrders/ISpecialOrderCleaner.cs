using StardewValley.GameData.SpecialOrders;
using StardewValley.SpecialOrders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Environment;

namespace OnlyFruitsMod.Features.Quests.SpecialOrders
{
    public interface ISpecialOrderCleaner
    {
        void PatchAsset(SpecialOrderData? specialOrderData);
        void PatchLiveData(SpecialOrder specialOrder);
    }

}
