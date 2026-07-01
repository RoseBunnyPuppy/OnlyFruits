using OnlyFruitsMod.Features.Logging;
using OnlyFruitsMod.Features.ModConfiguration;
using OnlyFruitsMod.Features.PerSaveChallengeInformation;
using OnlyFruitsMod.ModParts;
using OnlyFruitsMod.ModParts.Models;
using StardewModdingAPI;

namespace OnlyFruitsMod
{
    internal sealed class ModEntry : Mod
    {
        public ModConfigInstance? ConfigInstance { get; private set; }


        private ModPartContext BuildPartContext(
            IModHelper helper
        )
        {
            var configInstance = new ModConfigInstance(
                helper,
                this.Monitor
            );
            var perSaveChallengeInstance = new PerSaveChallengeInformationInstance(helper);
            var modPartContext = new ModPartContext(
                helper,
                this.Monitor,
                configInstance,
                this.ModManifest,
                perSaveChallengeInstance
            );
            return modPartContext;
        }
        
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Logger.Instance.SetMonitor(this.Monitor);

            var modPartContext = this.BuildPartContext(helper);
            this.ConfigInstance = modPartContext.ConfigInstance;

            // setup the UI handlers
            var _uiPart = new UIModPart(modPartContext);
            _uiPart.Run();

            // build the parts Data/Buildings, Data/NPCGiftTastes,  Data/LostItemsShop, Data/TriggerActions
            var _pricePart = new PriceModPart(modPartContext);
            var _shopsPart = new ShopsModPart(modPartContext);
            var _specialOrdersPart = new SpecialOrderModPart(modPartContext);
            var _questPart = new QuestModPart(modPartContext);
            var _monsterSlayerPart = new MonsterSlayerQuestsModPart(modPartContext);
            var challengeNoticePart = new ChallengeNoticeModPart(modPartContext);

            challengeNoticePart.Run();
            // run the parts
            _pricePart.Run();
            _shopsPart.Run();
            _specialOrdersPart.Run();
            _questPart.Run();
            _monsterSlayerPart.Run();
        }

    }
}
