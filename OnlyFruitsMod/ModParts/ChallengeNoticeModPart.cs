using OnlyFruitsMod.Features.Dialogue;
using OnlyFruitsMod.Features.PerSaveChallengeInformation;
using OnlyFruitsMod.Models.GameData;
using OnlyFruitsMod.ModParts.Core;
using OnlyFruitsMod.ModParts.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace OnlyFruitsMod.ModParts
{

    public class ChallengeNoticeModPart : ModPartBase
    {
        private readonly ChallengeWarningHelper challengeWarningHelper;

        public IManifest ModManifest => this.Context.ModManifest;

        private readonly DialogueMonitor dialogueMonitor;
        private bool previousAlwaysAskStatus = false;


        public ChallengeNoticeModPart(
            ModPartContext context
        ) : base(context)
        {
            this.dialogueMonitor = new DialogueMonitor(context);
            this.challengeWarningHelper = new ChallengeWarningHelper(
                this.Context,
                this.dialogueMonitor
            );
        }

        protected override void AttachListeners()
        {
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;
            this.Context.ConfigInstance.AlwaysAskAboutChallengeChanged += ConfigInstance_AlwaysAskAboutChallengeChanged; ;
        }

        private void GameLoop_ReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
        {
            var hlg = Game1.hasLoadedGame;
            var oldInfo = this.Context.PerSaveChallengeInstance.Information;
            this.Context.PerSaveChallengeInstance.UnsetPerSaveInfo();
            _ = 23;
        }

        private void ConfigInstance_AlwaysAskAboutChallengeChanged(object? sender, EventArgs e)
        {
            if (!Game1.hasLoadedGame) return;
            else if (!this.Context.ConfigInstance.Config.AlwaysAskWhetherToUseChallenge) return;
            this.monitor.Log($"The 'Always ask' status changed to true, asking whether to save", LogLevel.Debug);
            this.challengeWarningHelper.AutoHandleChallengeStatus();
        }

        private void GameLoop_SaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            this.challengeWarningHelper.AutoHandleChallengeStatus();
        }
    }

}