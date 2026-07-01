using OnlyFruitsMod.Features.UIHelpers;
using OnlyFruitsMod.Models;
using OnlyFruitsMod.ModParts.Core;
using OnlyFruitsMod.ModParts.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Integrations.GenericModConfigMenu;

namespace OnlyFruitsMod.ModParts
{
    /// <summary>
    ///   Configures and wires up the GMCM UI.
    /// </summary>
    public class UIModPart : ModPartBase
    {
        public IManifest ModManifest => this.Context.ModManifest;

        public UIModPart(
            ModPartContext context
        ) : base(context)
        {

        }
       


        protected override void AttachListeners()
        {
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
        }


     
        EnumChoiceMap<QuestReplacementModes> CropOrderChoiceMap = EnumChoiceMap.Create(new Dictionary<QuestReplacementModes, string>()
        {
            [QuestReplacementModes.NoMonetaryReward] = "no-money",
            [QuestReplacementModes.SwapWithFruits] = "fruit-crops",
        }, QuestReplacementModes.NoMonetaryReward, new QuestReplacementModes[]
        {
            QuestReplacementModes.NoMonetaryReward,
            QuestReplacementModes.SwapWithFruits,
        });


        private void GameLoop_GameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            var configMenu = this.helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null) return;

            ConfigMenuHelper configMenuHelper = new(
                this.ModManifest,
                this.helper,
                configMenu
            );

            configMenuHelper
                .FluentBlock(configMenu =>
                {
                    configMenu.Register(
                        mod: this.ModManifest,
                        reset: () => this.configInstance.Reset(),
                        save: () => this.configInstance.Save()
                    );
                })
                .AddParagraph(
                    i18nKey: "rosebunnypuppy.onlyfruits.ui.version-string",
                    tokens: new { ModVersion = this.ModManifest.Version }
                )
                // Sellable Configuration
                .AddSectionTitle("rosebunnypuppy.onlyfruits.ui.sellable-section")
                .AddBoolOption(
                    i18nKeyName: "rosebunnypuppy.onlyfruits.ui.sellable-section.option-meme-items",
                    getValue: () => configInstance.Config.AllowSellingMemeItems,
                    setValue: value => configInstance.Config.AllowSellingMemeItems = value
                )
                .AddBoolOption(
                    i18nKeyName: "rosebunnypuppy.onlyfruits.ui.sellable-section.option-should-be-fruits",
                    getValue: () => configInstance.Config.AllowSellingShouldaBeenFruitItems,
                    setValue: value => configInstance.Config.AllowSellingShouldaBeenFruitItems = value
                )
                .AddBoolOption(
                    i18nKeyName: "rosebunnypuppy.onlyfruits.ui.sellable-section.option-cooked-items",
                    getValue: () => configInstance.Config.AllowSellingAutoDerivedItems,
                    setValue: value => configInstance.Config.AllowSellingAutoDerivedItems = value
                )
                .AddBoolOption(
                    i18nKeyName: "rosebunnypuppy.onlyfruits.ui.sellable-section.option-fruity-artisinal-items",
                    getValue: () => configInstance.Config.AllowSellingArtisinalItems,
                    setValue: value => configInstance.Config.AllowSellingArtisinalItems = value
                )
                .AddBoolOption(
                    i18nKeyName: "rosebunnypuppy.onlyfruits.ui.sellable-section.option-no-nonfruity-shops",
                    getValue: () => configInstance.Config.PatchNonFruityShopItems,
                    setValue: value => configInstance.Config.PatchNonFruityShopItems = value
                )
                // question section
                .AddSectionTitle("rosebunnypuppy.onlyfruits.ui.questing-section")
                .AddBoolOption(
                    i18nKeyName: "rosebunnypuppy.onlyfruits.ui.questing-section.option-no-nonfruity-money",
                    getValue: () => configInstance.Config.Questing_NoMoneyFromNonFruityQuests,
                    setValue: value => configInstance.Config.Questing_NoMoneyFromNonFruityQuests = value
                )
                .AddBoolOption(
                    i18nKeyName: "rosebunnypuppy.onlyfruits.ui.questing-section.option-no-nonfruity-qi",
                    getValue: () => configInstance.Config.Questing_NoNonFruityQiQuests,
                    setValue: value => configInstance.Config.Questing_NoNonFruityQiQuests = value
                )
                .AddBoolOption(
                    i18nKeyName: "rosebunnypuppy.onlyfruits.ui.questing-section.option-no-nonfruity-monster-slayer",
                    getValue: () => configInstance.Config.Questing_NoMoneyFromNonFruityMonsterSlayerQuests,
                    setValue: value => configInstance.Config.Questing_NoMoneyFromNonFruityMonsterSlayerQuests = value
                )
                // quest-patch section
                .AddSectionTitle("rosebunnypuppy.onlyfruits.ui.quest-patching-section")
                .AddTextOption(
                    i18nKeyName: "rosebunnypuppy.onlyfruits.ui.quest-patching-section.option-lewis",
                    getValue: () => CropOrderChoiceMap.GetStringValue(configInstance.Config.Questing_PatchLewisCropOrderQuest),
                    setValue: value => configInstance.Config.Questing_PatchLewisCropOrderQuest = CropOrderChoiceMap.GetEnumValue(value),
                    allowedValues: CropOrderChoiceMap.GetAllowed()
                )
                .AddTextOption(
                    i18nKeyName: "rosebunnypuppy.onlyfruits.ui.quest-patching-section.option-caroline",
                    getValue: () => CropOrderChoiceMap.GetStringValue(configInstance.Config.Questing_PatchCarolineIslandIngredientsQuest),
                    setValue: value => configInstance.Config.Questing_PatchCarolineIslandIngredientsQuest = CropOrderChoiceMap.GetEnumValue(value),
                    allowedValues: CropOrderChoiceMap.GetAllowed()
                )
                // "other" section
                .AddSectionTitle("rosebunnypuppy.onlyfruits.ui.other-section")
                .AddBoolOption(
                    i18nKeyName: "rosebunnypuppy.onlyfruits.ui.other-section.option-restore-prices",
                    getValue: () => configInstance.Config.RestoreAllPrices,
                    setValue: value => configInstance.Config.RestoreAllPrices = value
                )
                .AddBoolOption(
                    i18nKeyName: "rosebunnypuppy.onlyfruits.ui.other-section.option-restore-quests",
                    getValue: () => configInstance.Config.RestoreAllQuestRewards,
                    setValue: value => configInstance.Config.RestoreAllQuestRewards = value
                )
                .AddBoolOption(
                    i18nKeyName: "rosebunnypuppy.onlyfruits.ui.other-section.option-allow-trashcan-upgrades",
                    getValue: () => configInstance.Config.AllowTrashcanUpgrade,
                    setValue: value => configInstance.Config.AllowTrashcanUpgrade = value
                )
                .AddSectionTitle("rosebunnypuppy.onlyfruits.ui.challenge-section")
                .AddBoolOption(
                    i18nKeyName: "rosebunnypuppy.onlyfruits.ui.challenge-section.option-always-ask-to-enable",
                    getValue: () => configInstance.Config.AlwaysAskWhetherToUseChallenge,
                    setValue: value => configInstance.Config.AlwaysAskWhetherToUseChallenge = value
                )
                .AddBoolOption(
                    i18nKeyName: "rosebunnypuppy.onlyfruits.ui.challenge-section.option-announce-when-challenge-enabled",
                    getValue: () => configInstance.Config.AnnounceWhenChallengeIsEnabled,
                    setValue: value => configInstance.Config.AnnounceWhenChallengeIsEnabled = value
                )
                .AddBoolOption(
                    i18nKeyName: "rosebunnypuppy.onlyfruits.ui.challenge-section.option-announce-when-challenge-disabled",
                    getValue: () => configInstance.Config.AnnounceWhenChallengeIsDisabled,
                    setValue: value => configInstance.Config.AnnounceWhenChallengeIsDisabled = value
                )

            ;
        }

    }
}
