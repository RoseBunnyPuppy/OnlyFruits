namespace OnlyFruitsMod.Features.Quests
{
    public interface IQuestPatchTester
    {
        bool IsPatchingQuest(string questId);
        bool IsPatchingDailyQuest();
    }
   
}
