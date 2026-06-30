using OnlyFruitsMod.Features.ModConfiguration;
using OnlyFruitsMod.Features.UIHelpers;
using OnlyFruitsMod.Models.GameData;
using OnlyFruitsMod.ModParts;
using OnlyFruitsMod.ModParts.Models;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System.Reflection.Metadata;

namespace OnlyFruitsMod.Features.PerSaveChallengeInformation
{
    public class ChallengeWarningHelper
    {
        const string EnableChoice = "enable_challenge";
        const string DisableChoice = "disable_challenge";


        private readonly IModHelper helper;
        private readonly ModConfigInstance configInstance;
        private readonly ModPartContext context;
        private readonly DialogueMonitor dialogueMonitor;

        public ChallengeWarningHelper(
            ModPartContext context,
            DialogueMonitor dialogueMonitor
        )
        {
            this.helper = context.Helper;
            this.configInstance = context.ConfigInstance;
            this.context = context;
            this.dialogueMonitor = dialogueMonitor;
        }
        
        /// <summary>
        ///     Enqueue or set the currently active clickable menu.
        ///   Does NOT directly use the <see cref="DialogueMonitor"/>
        /// </summary>
        private void SetDialogueMessage(string message)
        {
            var dialogueBox = new DialogueBox(message);
            // enqueue if there is an active or pending clickable menu
            if (Game1.activeClickableMenu != null || Game1.nextClickableMenu.Any())
            {
                Game1.nextClickableMenu.Add(dialogueBox);
            }
            // otherwise, immediately display
            else
            {
                Game1.activeClickableMenu = dialogueBox;
            }
        }
        
        private void AnnounceChallengeIsEnabled()
        {
            string message = this.helper.Translation.Get("rosebunnypuppy.onlyfruits.ui.challenge-warning.enabled");
            this.SetDialogueMessage(message);
        }
        private void AnnounceChallengeHasBeenFreshlyDisabled()
        {
            string message = this.helper.Translation.Get("rosebunnypuppy.onlyfruits.ui.challenge-warning.disabled_fresh");
            this.SetDialogueMessage(message);
        }
        private void AnnounceChallengeIsDisabled()
        {
            string message = this.helper.Translation.Get("rosebunnypuppy.onlyfruits.ui.challenge-warning.disabled");
            this.SetDialogueMessage(message);
        }


        private void SetEnabledStatus(bool status)
        {
            var current = this.context.PerSaveChallengeInstance.GetOrCreateChallengeInformation();
            current.IsEnabled = status;
            this.context.ConfigInstance.RaiseChanged();
        }

        public void AutoHandleChallengeStatus(bool force = false)
        {
            var statusInfo = this.helper.Data.ReadSaveData<OnlyFruitsChallengeStatusData>(OnlyFruitsChallengeStatusData.DataKey);
            
            var status = statusInfo?.IsEnabled;

            if (force || status == null)
            {
                this.AskIfTheChallengeShouldBeEnabled();
                return;
            }
            else if (this.configInstance.Config.AlwaysAskWhetherToUseChallenge)
            {
                this.AskIfTheChallengeShouldBeEnabled();
                return;
            }
            else if (status == true)
            {
                if (this.configInstance.Config.AnnounceWhenChallengeIsEnabled)
                {
                    this.AnnounceChallengeIsEnabled();
                }
                this.SetEnabledStatus(status.Value);
                return;
            }
            else
            {
                // dont announce if configured to do so
                if (this.configInstance.Config.AnnounceWhenChallengeIsDisabled)
                {
                    this.AnnounceChallengeIsDisabled();
                }
                this.SetEnabledStatus(status.Value);
                return;
            }
        }
        
        public void AskIfTheChallengeShouldBeEnabled()
        {
            this.dialogueMonitor.Enqueue(() =>
            {
                Game1.currentLocation.createQuestionDialogue(
                    question: this.helper.Translation.Get("rosebunnypuppy.onlyfruits.ui.challenge-warning.query.label"),
                    answerChoices: new Response[] {
                        new(EnableChoice, this.helper.Translation.Get("rosebunnypuppy.onlyfruits.ui.challenge-warning.query.option.enable")),
                        new(DisableChoice, this.helper.Translation.Get("rosebunnypuppy.onlyfruits.ui.challenge-warning.query.option.disable")),
                    },
                    afterDialogueBehavior: new GameLocation.afterQuestionBehavior(ResponseHandler)
                );
            });
        }

        /// <summary>
        ///   Mark the challenge as being enabled or disabled 'at some point'.
        /// </summary>
        private void WriteHasEverBeenEnabled(bool status)
        {
            this.helper.Data.WriteSaveData(OnlyFruitsChallengeEverEnableData.DataKey, new OnlyFruitsChallengeEverEnableData
            {
                Status = status,
            });
        }

        /// <summary>
        ///   Mark the challenge as currently being enabled or disabled.
        /// </summary>
        private void WriteIsCurrentlyEnabled(bool status)
        {
            this.helper.Data.WriteSaveData(OnlyFruitsChallengeStatusData.DataKey, new OnlyFruitsChallengeStatusData
            {
                IsEnabled = status,
            });
            this.SetEnabledStatus(status);
        }

        void ResponseHandler(Farmer who, string dialogueId)
        {
            switch (dialogueId)
            {
                case EnableChoice:
                {
                    // track that the challenge has _at some point_ been enabled.
                    this.WriteHasEverBeenEnabled(true);
                    // track that the challenge is currently enabled.
                    this.WriteIsCurrentlyEnabled(true);
                    // make any announcements
                    this.AnnounceChallengeIsEnabled();
                    return;
                }
                case DisableChoice:
                {
                    // track that the challenge is currently disabled.
                    this.WriteIsCurrentlyEnabled(false);
                    // make any announcements
                    this.AnnounceChallengeHasBeenFreshlyDisabled();
                    return;
                }
                default:
                    this.AskIfTheChallengeShouldBeEnabled();
                    break;
            }
        }
    }
}
