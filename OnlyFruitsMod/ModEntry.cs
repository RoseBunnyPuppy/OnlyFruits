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
            var modPartContext = new ModPartContext(
                helper,
                Logger.Instance,
                new ModConfigInstance(helper),
                this.ModManifest,
                new PerSaveChallengeInformationInstance(helper)
            );
            return modPartContext;
        }
        
        /// <summary>
        ///   Configure the <see cref="Logger.Instance"/>
        /// </summary>
        /// <exception cref="InvalidOperationException">The instance was an unexpected type. We cannot recover from this.</exception>
        private void InitializeLogger()
        {
            if (Logger.Instance is not Logger fullLogger)
            {
                this.Monitor.Log($"Expected the logger to be of type {nameof(Logger)}.  Was actually {Logger.Instance.GetType().FullName}", LogLevel.Error);
                throw new InvalidOperationException($"Expected the logger to be of type {nameof(Logger)}");
            }
            fullLogger.SetMonitor(this.Monitor);
        }
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.InitializeLogger();

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
