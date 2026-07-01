using OnlyFruitsMod.Features.ModConfiguration;
using OnlyFruitsMod.Features.PerSaveChallengeInformation;
using OnlyFruitsMod.Features.Quests.Models;
using OnlyFruitsMod.Infrastructure;
using OnlyFruitsMod.Models;

namespace OnlyFruitsMod.Features.Quests.SpecialOrders
{
    public class SpecialOrderStatusDeterminer
    {
        private readonly ModConfigInstance configInstance;
        public SpecialOrderKeysConfigModel SpecialOrderKeysConfigModel { get; }
        public PerSaveChallengeInformationInstance PerSaveChallengeInstance { get; }

        public SpecialOrderStatusDeterminer(
            ModConfigInstance configInstance,
            SpecialOrderKeysConfigModel specialOrderKeysConfigModel,
            PerSaveChallengeInformationInstance perSaveChallengeInstance
        )
        {
            this.configInstance = configInstance;
            SpecialOrderKeysConfigModel = specialOrderKeysConfigModel;
            PerSaveChallengeInstance = perSaveChallengeInstance;
        }


        public OrderPatchingFlavors GetPatchingFlavor(string questId)
        {
            // dont patch if the challenge isnt enabled.
            if (!this.PerSaveChallengeInstance.IsChallengeEnabled) return OrderPatchingFlavors.DontPatch;
            // if we are restoring the quests, don't patch anything
            if (this.configInstance.Config.RestoreAllQuestRewards) return OrderPatchingFlavors.DontPatch;
            // if this is a non-fruity quest
            else if (this.SpecialOrderKeysConfigModel.AlwaysResetMoney.Contains(questId))
            {
                // and we _ARENT_ resetting non-fruity quests
                if (!this.configInstance.Config.Questing_NoMoneyFromNonFruityQuests)
                    return OrderPatchingFlavors.DontPatch;
                // otherwise, patch
                return OrderPatchingFlavors.NonFruity;
            }
            // if this is an always-disabled qi quest
            else if (this.SpecialOrderKeysConfigModel.AlwaysDisabledQi.Contains(questId))
            {
                // and we _ARENT_ patching those
                if (!this.configInstance.Config.Questing_NoNonFruityQiQuests) return OrderPatchingFlavors.DontPatch;
                return OrderPatchingFlavors.NonFruityQi;
            }
            // if this is a potentially-disabled qi quest
            else if (this.SpecialOrderKeysConfigModel.PotentiallyDisabledQi.Contains(questId))
            {
                // and we _ARENT_ patching those
                if (!this.configInstance.Config.Questing_NoNonFruityQiQuests) return OrderPatchingFlavors.DontPatch;
                return OrderPatchingFlavors.PotentiallyNonFruityQi;
            }
            else if (questId == HardcodedQuestConstants.Lewis)
            {
                return this.configInstance.Config.Questing_PatchLewisCropOrderQuest switch
                {
                    QuestReplacementModes.NoMonetaryReward => OrderPatchingFlavors.NonFruity,
                    QuestReplacementModes.SwapWithFruits => OrderPatchingFlavors.LewisSpecialOrder,
                    _ => OrderPatchingFlavors.NonFruity,
                };
            }
            else if (questId == HardcodedQuestConstants.Caroline)
            {
                return this.configInstance.Config.Questing_PatchCarolineIslandIngredientsQuest switch
                {
                    QuestReplacementModes.NoMonetaryReward => OrderPatchingFlavors.NonFruity,
                    QuestReplacementModes.SwapWithFruits => OrderPatchingFlavors.CarolineSpecialOrder,
                    _ => OrderPatchingFlavors.NonFruity,
                };
            }
            else return OrderPatchingFlavors.DontPatch;
        }

    }
}