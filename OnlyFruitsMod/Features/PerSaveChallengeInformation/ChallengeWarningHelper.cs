using OnlyFruitsMod.Features.Dialogue;
using OnlyFruitsMod.Features.ModConfiguration;
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
        
        private void EnqueueDialogueMessage(string message)
        {
            var hasActive = this.dialogueMonitor.HasActiveDialogue;
            var active = Game1.activeClickableMenu;
            var next = Game1.nextClickableMenu;
            var dialogueBox = new DialogueBox(message);
            if (Game1.activeClickableMenu != null || Game1.nextClickableMenu.Any())
            {
                Game1.nextClickableMenu.Add(dialogueBox);
                return;
            }
            else
            {
                Game1.activeClickableMenu = dialogueBox;
            }
        }
        private void AnnounceChallengeIsEnabled()
        {
            string message = this.helper.Translation.Get("rosebunnypuppy.onlyfruits.ui.challenge-warning.enabled");
            this.EnqueueDialogueMessage(message);
        }
        private void AnnounceChallengeHasBeenFreshlyDisabled()
        {
            string message = this.helper.Translation.Get("rosebunnypuppy.onlyfruits.ui.challenge-warning.disabled_fresh");
            this.EnqueueDialogueMessage(message);
        }
        private void AnnounceChallengeIsDisabled()
        {
            string message = this.helper.Translation.Get("rosebunnypuppy.onlyfruits.ui.challenge-warning.disabled");
            this.EnqueueDialogueMessage(message);
        }


        private void SetEnabledStatus(bool status)
        {
            var current = this.context.PerSaveChallengeInstance.Information ?? new PerSaveChallengeInformation();
            if (current != this.context.PerSaveChallengeInstance.Information)
                this.context.PerSaveChallengeInstance.Information = current;
            current.IsEnabled = status;
            this.context.ConfigInstance.RaiseChanged();
        }

        public void AutoHandleChallengeStatus()
        {
            var statusInfo = this.helper.Data.ReadSaveData<OnlyFruitsChallengeStatusData>(OnlyFruitsChallengeStatusData.DataKey);


            var status = statusInfo?.IsEnabled;

            if (status == null)
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

        private void WriteHasEverBeenEnabled(bool status)
        {
            this.helper.Data.WriteSaveData(OnlyFruitsChallengeEverEnableData.DataKey, new OnlyFruitsChallengeEverEnableData
            {
                Status = status,
            });
        }
        private void WriteIsCurrentlyEnabled(bool status)
        {
            this.helper.Data.WriteSaveData(OnlyFruitsChallengeStatusData.DataKey, new OnlyFruitsChallengeStatusData
            {
                IsEnabled = status,
            });
        }

        void ResponseHandler(Farmer who, string dialogueId)
        {
            switch (dialogueId)
            {
                case EnableChoice:
                {
                    this.WriteHasEverBeenEnabled(true);
                    this.WriteIsCurrentlyEnabled(true);
                    
                    this.SetEnabledStatus(true);
                    this.AnnounceChallengeIsEnabled();
                    return;
                }
                case DisableChoice:
                {
                    
                    this.WriteIsCurrentlyEnabled(false);
                    this.SetEnabledStatus(false);
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
