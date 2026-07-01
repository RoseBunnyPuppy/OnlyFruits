using Netcode;
using StardewValley.Quests;

namespace OnlyFruitsMod.Features.Quests.Models
{
    public class OnlyFruitsSlayMonsterQuest : SlayMonsterQuest
    {
        public OnlyFruitsSlayMonsterQuest()
            : base()
        {
            this.reward.Value = 0;
            this.moneyReward.Value = 0;
            this.reward.fieldChangeEvent += FieldValueEventHandlers.ResetFieldToZeroHandler;
        }
    }

}
