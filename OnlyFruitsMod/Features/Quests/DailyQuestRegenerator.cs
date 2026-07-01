using Netcode;
using OnlyFruitsMod.Extensions;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Quests;
using System.Security.AccessControl;

namespace OnlyFruitsMod.Features.Quests
{
    public class DailyQuestRegenerator : IDailyQuestRegenerator
    {

        public static string QOTDOriginalReward { get; } = "RoseBunnyPuppy.OnlyFruits:QOTDOriginalReward";

        /// <summary>
        ///     Copy over the values from the old quest, 
        ///   lock those values, reload using the locked values,
        ///   and then unlock the values.
        /// </summary>
        private void CopySlayQuest(SlayMonsterQuest freshQuest, SlayMonsterQuest oldQuest)
        {
            // copy over the old values
            freshQuest.target.Value = oldQuest.target.Value;
            freshQuest.monsterName.Value = oldQuest.monsterName.Value;
            freshQuest.numberKilled.Value = oldQuest.numberKilled.Value;
            freshQuest.numberToKill.Value = oldQuest.numberToKill.Value;
            freshQuest.daysLeft.Value = oldQuest.daysLeft.Value;
            freshQuest.dayQuestAccepted.Value = oldQuest.dayQuestAccepted.Value;

            // lock the fields, invoke the action, and auto-unlock
            freshQuest.reward.WithLockedField(() =>
            freshQuest.target.WithLockedField(() =>
            freshQuest.monsterName.WithLockedField(() =>
            freshQuest.numberKilled.WithLockedField(() =>
            freshQuest.numberToKill.WithLockedField(() =>
            freshQuest.daysLeft.WithLockedField(() =>
            freshQuest.dayQuestAccepted.WithLockedField(() =>
            {
                // initialize using the locked fields
                freshQuest.loadQuestInfo();
            })))))));


            /* 
             * NOTE: the following grossness is copied from the decompiled 
             * source of 'SlayMonsterQuest::loadQuestInfo
             */
            freshQuest.parts.Clear();
            freshQuest.parts.AddRange(oldQuest.parts.SkipLast(1));
            freshQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13274", freshQuest.reward.Value));

            freshQuest.dialogueparts.Clear();
            freshQuest.dialogueparts.AddRange(oldQuest.dialogueparts);

            freshQuest.objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13770", "0", freshQuest.numberToKill.Value, freshQuest.monster.Value);

            freshQuest.reloadDescription();
            freshQuest.reloadObjective();
        }
        private void CopyResourceCollectionQuest(
            ResourceCollectionQuest freshQuest,
            ResourceCollectionQuest oldQuest
        )
        {
            freshQuest.ItemId.Value = oldQuest.ItemId.Value;
            freshQuest.target.Value = oldQuest.target.Value;
            freshQuest.daysLeft.Value = oldQuest.daysLeft.Value;
            freshQuest.dayQuestAccepted.Value = oldQuest.dayQuestAccepted.Value;
            freshQuest.numberCollected.Value = oldQuest.numberCollected.Value;
            freshQuest.number.Value = oldQuest.number.Value;

            // lock the fields, invoke the action, and auto-unlock
            freshQuest.reward.WithLockedField(() =>
            freshQuest.ItemId.WithLockedField(() =>
            freshQuest.target.WithLockedField(() =>
            freshQuest.daysLeft.WithLockedField(() =>
            freshQuest.dayQuestAccepted.WithLockedField(() =>
            freshQuest.numberCollected.WithLockedField(() =>
            freshQuest.number.WithLockedField(() =>
            {
                freshQuest.loadQuestInfo();
            })))))));

            /* 
             * NOTE: the following grossness is copied from the decompiled 
             * source of 'ResourceCollectionQuest::loadQuestInfo
             */
            Item item = ItemRegistry.Create(freshQuest.ItemId.Value);
            freshQuest.parts.Clear();
            freshQuest.parts.Add(oldQuest.parts[0]);
            freshQuest.dialogueparts.Clear();
            freshQuest.dialogueparts.AddRange(oldQuest.dialogueparts);
            freshQuest.parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13607", freshQuest.reward.Value));
            freshQuest.parts.Add(freshQuest.target.Value.Equals("Clint") ? "Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13688" : "");
            freshQuest.objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:ResourceCollectionQuest.cs.13691", "0", freshQuest.number.Value, item);
            freshQuest.reloadDescription();
            freshQuest.reloadObjective();
        }

        private void ConfigureOnlyFruitsQOTD(Quest quest, bool markAsOnlyFruitQuest)
        {
            quest.dailyQuest.Value = true;
            if (markAsOnlyFruitQuest)
            {
                quest.SetOnlyFruitsQOTDStatus();
            }
            Game1.netWorldState.Value.SetQuestOfTheDay(quest);
        }
        private int GetOriginalRewardAmount(SlayMonsterQuest quest)
        {
            if (quest.modData.TryGetValue(QOTDOriginalReward, out var field))
                return int.Parse(field);

            return quest.reward.Value;
        }
        private int GetOriginalRewardAmount(ResourceCollectionQuest quest)
        {
            if (quest.modData.TryGetValue(QOTDOriginalReward, out var field))
                return int.Parse(field);

            return quest.reward.Value;
        }
        public Quest RegenerateQuest(Quest quest, bool markAsOnlyFruitQuest)
        {
            // think this is good because it rewards based on the sell price of the items
            if (quest is ItemDeliveryQuest _)
            {
                var freshQuest = new ItemDeliveryQuest();
                freshQuest.loadQuestInfo();
                this.ConfigureOnlyFruitsQOTD(freshQuest, markAsOnlyFruitQuest);
                freshQuest.accepted.Value = quest.accepted.Value;
                return freshQuest;
            }
            // think this is good because it rewards based on the sell price of the items
            else if (quest is FishingQuest _)
            {
                var freshQuest = new FishingQuest();
                freshQuest.loadQuestInfo();
                this.ConfigureOnlyFruitsQOTD(freshQuest, markAsOnlyFruitQuest);
                freshQuest.accepted.Value = quest.accepted.Value;
                return freshQuest;
            }
            // broken
            else if (quest is SlayMonsterQuest oldSlayMonsterQuest)
            {
                var freshQuest = new SlayMonsterQuest();
                if (markAsOnlyFruitQuest)
                {
                    var originalReward = this.GetOriginalRewardAmount(oldSlayMonsterQuest);
                    freshQuest.modData[QOTDOriginalReward] = originalReward.ToString();
                    freshQuest.reward.Value = 0;
                }
                else
                {
                    freshQuest.reward.Value = this.GetOriginalRewardAmount(oldSlayMonsterQuest);
                }

                this.CopySlayQuest(freshQuest, oldQuest: oldSlayMonsterQuest);
                this.ConfigureOnlyFruitsQOTD(freshQuest, markAsOnlyFruitQuest);
                freshQuest.accepted.Value = quest.accepted.Value;
                return freshQuest;
            }
            // broken
            else if (quest is ResourceCollectionQuest oldResourceCollectionQuest)
            {
                var freshQuest = new ResourceCollectionQuest();
                if (markAsOnlyFruitQuest)
                {
                    var originalReward = this.GetOriginalRewardAmount(oldResourceCollectionQuest);
                    freshQuest.modData[QOTDOriginalReward] = originalReward.ToString();
                    freshQuest.reward.Value = 0;
                }
                else
                {
                    freshQuest.reward.Value = this.GetOriginalRewardAmount(oldResourceCollectionQuest);
                }
              
                this.CopyResourceCollectionQuest(freshQuest, oldQuest: oldResourceCollectionQuest);
                this.ConfigureOnlyFruitsQOTD(freshQuest, markAsOnlyFruitQuest);
                freshQuest.accepted.Value = quest.accepted.Value;
                return freshQuest;
            }
            return quest;
        }

        public bool RestoreReward(Quest quest, NetInt rewardField)
        {
            // abort if there is no directly stored cached reward
            if (!quest.TryGetDirectlyCachedModDataQuestReward(out var directReward)) return false;

            // otherwise, apply the cached value
            rewardField.Value = directReward;
            return true;
        }
    }

}
