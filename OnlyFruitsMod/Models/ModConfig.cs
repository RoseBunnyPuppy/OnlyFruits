namespace OnlyFruitsMod.Models
{
    public enum QuestReplacementModes
    {
        NoMonetaryReward = 2,
        SwapWithFruits = 3,
    }
    public sealed class ModConfig
    {
        #region "Sellable Section"
        /// <summary>
        ///   If true, meme items are sellable.
        /// </summary>
        /// <remarks>rosebunnypuppy.onlyfruits.ui.sellable-section.option-meme-items</remarks>
        public bool AllowMemeItems { get; set; } = true;

        /// <summary>
        ///   If true, items that use a fruit as an input are sellable.
        /// </summary>
        /// <remarks>rosebunnypuppy.onlyfruits.ui.sellable-section.option-cooked-items</remarks>
        public bool AllowAutoDerivedItems { get; set; } = true;

        /// <summary>
        ///   If true, items defined in the "manual derivative" list are sellable.
        /// </summary>
        /// <remarks>rosebunnypuppy.onlyfruits.ui.sellable-section.option-fruity-artisinal-items</remarks>
        public bool AllowManualDerivedItems { get; set; } = true;

        /// <summary>
        ///   If true, items that should be considered fruits are sellable.
        /// </summary>
        /// <remarks>rosebunnypuppy.onlyfruits.ui.sellable-section.option-should-be-fruits</remarks>
        public bool AllowShouldaBeenFruitItems { get; set; } = true;


        /// <summary>
        /// 
        /// </summary>
        /// <remarks>rosebunnypuppy.onlyfruits.ui.sellable-section.option-no-nonfruity-shops</remarks>
        public bool PatchNonFruityShopItems { get; set; } = true;
        #endregion "Sellable Section"


        #region "Questing Section"
        /// <summary>
        ///   If true, non-fruity qi-quests will not give any results.
        /// </summary>
        /// <remarks>rosebunnypuppy.onlyfruits.ui.questing-section.option-no-nonfruity-qi</remarks>
        public bool Questing_NoNonFruityQiQuests { get; set; } = true;

        /// <summary>
        ///   If true, non-fruity quests will not give any reward money.
        /// </summary>
        /// <remarks>rosebunnypuppy.onlyfruits.ui.questing-section.option-no-nonfruity-money</remarks>
        public bool Questing_NoMoneyFromNonFruityQuests { get; set; } = true;



        /// <summary>
        /// 
        /// </summary>
        /// <remarks>rosebunnypuppy.onlyfruits.ui.questing-section.option-no-nonfruity-monster-slayer</remarks>
        public bool Questing_NoMoneyFromNonFruityMonsterSlayerQuests { get; set; } = true;
        #endregion "Questing Section"


        #region "Quest Patching Section"
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>rosebunnypuppy.onlyfruits.ui.quest-patching-section.option-lewis</remarks>
        public QuestReplacementModes Questing_PatchLewisCropOrderQuest { get; set; } = QuestReplacementModes.SwapWithFruits;

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>rosebunnypuppy.onlyfruits.ui.quest-patching-section.option-caroline</remarks>
        public QuestReplacementModes Questing_PatchCarolineIslandIngredientsQuest { get; set; } = QuestReplacementModes.SwapWithFruits;

        #endregion "Quest Patching Section"


        #region "Other Section"
        /// <summary>
        ///   If true, attempt to restore all cached prices.
        /// </summary>
        /// <remarks>rosebunnypuppy.onlyfruits.ui.other-section.option-restore-prices</remarks>
        public bool RestoreAllPrices { get; set; } = false;

        /// <summary>
        /// </summary>
        /// <remarks>rosebunnypuppy.onlyfruits.ui.other-section.option-restore-quests</remarks>
        public bool RestoreAllQuestRewards { get; set; } = false;
        #endregion "Other Section"





    }
}
