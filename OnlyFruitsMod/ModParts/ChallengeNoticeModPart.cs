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
#if !DisableDevHelpers
            helper.Events.Input.ButtonReleased += Input_ButtonReleased;
#endif
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;
            this.Context.ConfigInstance.AlwaysAskAboutChallengeChanged += ConfigInstance_AlwaysAskAboutChallengeChanged; ;
        }
#if !DisableDevHelpers

        private void Input_ButtonReleased(object? sender, ButtonReleasedEventArgs e)
        {
            if (e.Button == SButton.F7)
            {
            }
            else if (e.Button == SButton.F8)
            {
                this.challengeWarningHelper.AutoHandleChallengeStatus(force: true);
            }
        }
#endif
        /// <summary>
        ///   Clear the per-save info when the player goes from 'within a game' to title.
        /// </summary>
        private void GameLoop_ReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
        {
            this.Context.PerSaveChallengeInstance.UnsetPerSaveInfo();
        }

        /// <summary>
        ///   Re-display the 'challenge notifier' when one of the appropriate settings changes.
        /// </summary>
        private void ConfigInstance_AlwaysAskAboutChallengeChanged(object? sender, EventArgs e)
        {
            // do nothing if no save is loaded
            if (!Game1.hasLoadedGame) return;
            // do nothing if the 'always ask' preference isnt set
            else if (!this.Context.ConfigInstance.Config.AlwaysAskWhetherToUseChallenge) return;

            this.challengeWarningHelper.AutoHandleChallengeStatus();
        }

        /// <summary>
        ///   Attempt to display the 'challenge notifier' when a new save is loaded.
        /// </summary>
        private void GameLoop_SaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            this.challengeWarningHelper.AutoHandleChallengeStatus();
        }
    }

}