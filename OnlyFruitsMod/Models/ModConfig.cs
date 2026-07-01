namespace OnlyFruitsMod.Models
{
    public enum QuestReplacementModes
    {
        NoMonetaryReward = 2,
        SwapWithFruits = 3,
    }
    public enum OnlyFruitsLogLevels
    {
        /// <summary>
        ///   Show none.
        /// </summary>
        None = 1,
        /// <summary>
        ///   Show every log.
        /// </summary>
        All = 2,
        /// <summary>
        ///   Show errors, info, and debug.
        /// </summary>
        Debug = 20,
        /// <summary>
        ///   Show errors and info.
        /// </summary>
        Info = 30,
        /// <summary>
        ///   Show only errors.
        /// </summary>
        Error = 40,
    }
    public sealed class ModConfig
    {
#if DEBUGPUBLISH
        public const OnlyFruitsLogLevels DefaultLogLevel = OnlyFruitsLogLevels.Debug;
#elif DEBUG
        public const OnlyFruitsLogLevels DefaultLogLevel = OnlyFruitsLogLevels.All;
#endif
        #region "Sellable Section"
        /// <summary>
        ///   If true, meme items are sellable.
        /// </summary>
        /// <remarks>rosebunnypuppy.onlyfruits.ui.sellable-section.option-meme-items</remarks>
        public bool AllowSellingMemeItems { get; set; } = true;

        /// <summary>
        ///   If true, items that use a fruit as an input are sellable.
        /// </summary>
        /// <remarks>rosebunnypuppy.onlyfruits.ui.sellable-section.option-cooked-items</remarks>
        public bool AllowSellingAutoDerivedItems { get; set; } = true;

        /// <summary>
        ///   If true, items defined in the "manual artisinal" list are sellable.
        /// </summary>
        /// <remarks>rosebunnypuppy.onlyfruits.ui.sellable-section.option-fruity-artisinal-items</remarks>
        public bool AllowSellingArtisinalItems { get; set; } = true;

        /// <summary>
        ///   If true, items that should be considered fruits are sellable.
        /// </summary>
        /// <remarks>rosebunnypuppy.onlyfruits.ui.sellable-section.option-should-be-fruits</remarks>
        public bool AllowSellingShouldaBeenFruitItems { get; set; } = true;


        /// <summary>
        ///   If true, item-shops wont buy their normal non-fruity stuff.
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
        ///   If true, the non-fruity monster slayer quests wont reward the player with money.
        /// </summary>
        /// <remarks>rosebunnypuppy.onlyfruits.ui.questing-section.option-no-nonfruity-monster-slayer</remarks>
        public bool Questing_NoMoneyFromNonFruityMonsterSlayerQuests { get; set; } = true;
        #endregion "Questing Section"


        #region "Quest Patching Section"
        /// <summary>
        ///   Configures the lewis quest.
        /// </summary>
        /// <remarks>rosebunnypuppy.onlyfruits.ui.quest-patching-section.option-lewis</remarks>
        public QuestReplacementModes Questing_PatchLewisCropOrderQuest { get; set; } = QuestReplacementModes.SwapWithFruits;

        /// <summary>
        ///   Configures the caroline quest.
        /// </summary>
        /// <remarks>rosebunnypuppy.onlyfruits.ui.quest-patching-section.option-caroline</remarks>
        public QuestReplacementModes Questing_PatchCarolineIslandIngredientsQuest { get; set; } = QuestReplacementModes.SwapWithFruits;

        #endregion "Quest Patching Section"


        #region "Other Section"
        /// <summary>
        ///     If true, trashcan upgrades will be allowed.  If false, trashcans will be capped at
        ///   the basic tier.
        /// </summary>
        /// <remarks>rosebunnypuppy.onlyfruits.ui.other-section.option-allow-trashcan-upgrades</remarks>
        public bool AllowTrashcanUpgrade { get; set; } = false;

        /// <summary>
        ///   Configures how verbose the logging is.
        /// </summary>
        /// <remarks>rosebunnypuppy.onlyfruits.ui.other-section.option-logging-verbosity</remarks>
        public OnlyFruitsLogLevels LoggingLevel { get; set; } = DefaultLogLevel;


        #endregion "Other Section"

        #region "Challenge Section"

        /// <summary>
        ///     If true, the "do you want to enable the challenge" question will
        ///   always be asked.
        /// </summary>
        /// <remarks>rosebunnypuppy.onlyfruits.ui.challenge-section.option-always-ask-to-use</remarks>
        public bool AlwaysAskWhetherToUseChallenge { get; set; } = false;

        /// <summary>
        ///     If true, a message will be displayed on save games where we ARE
        ///   using the challenge.
        /// </summary>
        /// <remarks>rosebunnypuppy.onlyfruits.ui.challenge-section.option-announce-when-challenge-enabled</remarks>
        public bool AnnounceWhenChallengeIsEnabled { get; set; } = true;

        /// <summary>
        ///     If true, a message will be displayed on save games where we AREN'T
        ///   using the challenge.
        /// </summary>
        /// <remarks>rosebunnypuppy.onlyfruits.ui.challenge-section.option-announce-when-challenge-disabled</remarks>
        public bool AnnounceWhenChallengeIsDisabled { get; set; } = false;
        #endregion "Challenge Section"





    }
}
