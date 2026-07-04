namespace OnlyFruitsMod.Infrastructure
{
    public static class HardcodedModDataKeys
    {
        const string ModScope = "RoseBunnyPuppy.OnlyFruits";
        public static string OriginalQuestOfTheDayReward { get; } = $"{ModScope}:OriginalQuestOfTheDayReward";
        public static string IsOnlyFruitsQuestOfTheDay { get; } = $"{ModScope}:IsOnlyFruitsQuestOfTheDay";
        public static string OriginalTrashCanDataKey { get; } = $"{ModScope}:OrigTrashCanLevel";
        public static string OriginalPriceModDataKey { get; } = $"{ModScope}:OrigPrice";
        public static string OriginalQuestRewardModDataKey { get; } = $"{ModScope}:OriginalReward";

        public static string CreateScoped(string name) => $"{ModScope}:{name}";
    }
}
