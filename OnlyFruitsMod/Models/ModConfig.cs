using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyFruitsMod.Models
{
    public enum QuestReplacementModes
    {
        NoMonetaryReward = 2,
        SwapWithFruits = 3,
    }
    public sealed class ModConfig
    {
        public const int ExpectedVersion = 1;

        public int Version { get; set; } = 0;
        public bool IsDefault { get; set; } = true;

        /// <summary>
        ///   If true, attempt to restore all cached prices.
        /// </summary>
        public bool RestoreAllPrices { get; set; } = false;
        public bool RestoreAllShops { get; set; } = false;
        public bool RestoreAllQuestRewards { get; set; } = false;


        /// <summary>
        ///   If true, meme items are sellable.
        /// </summary>
        public bool AllowMemeItems { get; set; } = true;

        /// <summary>
        ///   If true, items that use a fruit as an input are sellable.
        /// </summary>
        public bool AllowAutoDerivedItems { get; set; } = true;

        /// <summary>
        ///   If true, items defined in the "manual derivative" list are sellable.
        /// </summary>
        public bool AllowManualDerivedItems { get; set; } = true;

        /// <summary>
        ///   If true, items that should be considered fruits are sellable.
        /// </summary>
        public bool AllowShouldaBeenFruitItems { get; set; } = true;

        /// <summary>
        ///   If true, non-fruity qi-quests will not give any results.
        /// </summary>
        public bool Questing_NoNonFruityQiQuests { get; set; } = true;

        /// <summary>
        ///   If true, non-fruity quests will not give any reward money.
        /// </summary>
        public bool Questing_NoMoneyFromNonFruityQuests { get; set; } = true;
        public QuestReplacementModes Questing_PatchLewisCropOrderQuest { get; set; } = QuestReplacementModes.SwapWithFruits;
        public QuestReplacementModes Questing_PatchCarolineIslandIngredientsQuest { get; set; } = QuestReplacementModes.SwapWithFruits;
        public bool Questing_NoMoneyFromNonFruityMonsterSlayerQuests { get; set; } = true;

        public bool PatchNonFruityShopItems { get; set; } = true;
    }
}
