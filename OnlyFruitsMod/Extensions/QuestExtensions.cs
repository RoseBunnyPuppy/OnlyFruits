using OnlyFruitsMod.Infrastructure;
using StardewValley.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyFruitsMod.Extensions
{
    public static class QuestExtensions
    {
        public static void SetOnlyFruitsQOTDStatus(this Quest quest) =>
            quest.modData[HardcodedModDataKeys.IsOnlyFruitsQuestOfTheDay] = "true";


        public static bool IsOnlyFruitsQOTD(this Quest quest) => 
            quest.modData.TryGetValue(HardcodedModDataKeys.IsOnlyFruitsQuestOfTheDay, out _);
    }
}
