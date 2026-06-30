using OnlyFruitsMod.Features.PerSaveChallengeInformation;
using OnlyFruitsMod.Features.UIHelpers;
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
            this.Context.PerSaveChallengeInstance.UnsetPerSaveInfo();
        }

        private void ConfigInstance_AlwaysAskAboutChallengeChanged(object? sender, EventArgs e)
        {
            if (!Game1.hasLoadedGame) return;
            else if (!this.Context.ConfigInstance.Config.AlwaysAskWhetherToUseChallenge) return;
            this.challengeWarningHelper.AutoHandleChallengeStatus();
        }

        private void GameLoop_SaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            this.challengeWarningHelper.AutoHandleChallengeStatus();
        }
    }

}