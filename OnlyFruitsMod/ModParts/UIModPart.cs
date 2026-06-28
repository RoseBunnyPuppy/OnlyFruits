using OnlyFruitsMod.Features.ModConfiguration;
using OnlyFruitsMod.Features.UIHelpers;
using OnlyFruitsMod.Infrastructure;
using OnlyFruitsMod.Models;
using OnlyFruitsMod.ModParts.Core;
using OnlyFruitsMod.ModParts.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Integrations.GenericModConfigMenu;
using StardewValley;
using StardewValley.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

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
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched; ;
        }


        EnumChoiceMap<QuestReplacementModes> CropOrderChoiceMap = EnumChoiceMap.Create(new Dictionary<QuestReplacementModes, string>()
        {
            [QuestReplacementModes.NoMonetaryReward] = "No Money",
            [QuestReplacementModes.SwapWithFruits] = "Use Fruit Crops",
        }, QuestReplacementModes.NoMonetaryReward, new QuestReplacementModes[]
        {
            QuestReplacementModes.NoMonetaryReward,
            QuestReplacementModes.SwapWithFruits,
        });


        private void RegisterSellingSection(IGenericModConfigMenuApi configMenu)
        {
            // Sellable Configuration
            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => "Sellable Configuration",
                tooltip: () => "Configure the stuff that can be sold."
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Meme Items",
                tooltip: () => "If enabled, meme items (like ch- ch- cherry bombs) will be sellable.",
                getValue: () => configInstance.Config.AllowMemeItems,
                setValue: value => configInstance.Config.AllowMemeItems = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Shoulda-been-fruits",
                tooltip: () => "If enabled, items that should logically be fruits will be sellable. (Sweet-gem berries)",
                getValue: () => configInstance.Config.AllowShouldaBeenFruitItems,
                setValue: value => configInstance.Config.AllowShouldaBeenFruitItems = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Cooked Items",
                tooltip: () => "If enabled, items that use a fruit as an input will be sellable.",
                getValue: () => configInstance.Config.AllowAutoDerivedItems,
                setValue: value => configInstance.Config.AllowAutoDerivedItems = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Fruity Artisinal Items",
                tooltip: () => "If enabled, artisinal items that are fruit-based will be sellable. (Jelly, Wine, Raisins, and Dried Fruit)",
                getValue: () => configInstance.Config.AllowManualDerivedItems,
                setValue: value => configInstance.Config.AllowManualDerivedItems = value
            );
        }

        private void RegisterOtherSection(IGenericModConfigMenuApi configMenu)
        {
            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => "Other Settings",
                tooltip: () => "Configure other shit."
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Restore Prices",
                tooltip: () => "Attempt to apply the cached prices for all items.",
                getValue: () => configInstance.Config.RestoreAllPrices,
                setValue: value => configInstance.Config.RestoreAllPrices = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Restore Quests",
                tooltip: () => "Attempt to apply the cached rewards for all quests.",
                getValue: () => configInstance.Config.RestoreAllQuestRewards,
                setValue: value => configInstance.Config.RestoreAllQuestRewards = value
            );
        }
        private void RegisterQuestingSection(IGenericModConfigMenuApi configMenu)
        {
            // Non-fruity Quest Rewards
            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => "Non-fruity Quest Rewards",
                tooltip: () => "Configure rewards for non-fruity quests."
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "No money",
                tooltip: () => "If enabled, quests that dont have to do with fruits will be doable, but will not reward any money.",
                getValue: () => configInstance.Config.Questing_NoMoneyFromNonFruityQuests,
                setValue: value => configInstance.Config.Questing_NoMoneyFromNonFruityQuests = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "No non-fruity Qi",
                tooltip: () => "If enabled, qi quests that dont have to do with fruits will be doable, but will not reward any gems.",
                getValue: () => configInstance.Config.Questing_NoNonFruityQiQuests,
                setValue: value => configInstance.Config.Questing_NoNonFruityQiQuests = value
            );

            // Specific Quest Patching
            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => "Quest Patching",
                tooltip: () => "Configure specific quests that aren't fully fruity."
            );
            configMenu.AddTextOption(
                mod: this.ModManifest,
                name: () => "Lewis Quests",
                tooltip: () => "Configure the potential crops for Lewis's \"Crop Order\" Special Order. Either disable money rewards or replace the non-fruity crops with appropriate fruity crops.",
                getValue: () => CropOrderChoiceMap.GetStringValue(configInstance.Config.Questing_PatchLewisCropOrderQuest),
                setValue: value => configInstance.Config.Questing_PatchLewisCropOrderQuest = CropOrderChoiceMap.GetEnumValue(value),
                allowedValues: CropOrderChoiceMap.GetAllowed()
            );
            configMenu.AddTextOption(
                mod: this.ModManifest,
                name: () => "Caroline Quests",
                tooltip: () => "Configure the potential crops for Caroline's \"Island Ingredients\" Special Order. Either disable money rewards or replace the non-fruity crops with appropriate fruity crops.",
                getValue: () => CropOrderChoiceMap.GetStringValue(configInstance.Config.Questing_PatchCarolineIslandIngredientsQuest),
                setValue: value => configInstance.Config.Questing_PatchCarolineIslandIngredientsQuest = CropOrderChoiceMap.GetEnumValue(value),
                allowedValues: CropOrderChoiceMap.GetAllowed()
           );
        }


        private void GameLoop_GameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            var configMenu = this.helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null) return;

            configMenu.Register(
                mod: this.ModManifest,
                reset: () => this.configInstance.Reset(),
                save: () => this.configInstance.Save()
            );

            this.RegisterSellingSection(configMenu);
            this.RegisterQuestingSection(configMenu);
            this.RegisterOtherSection(configMenu);
        }

    }
}
