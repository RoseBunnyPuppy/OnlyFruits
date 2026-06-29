namespace OnlyFruitsMod.Infrastructure
{
    public static class PreloadConfiguration
    {
#if DEBUG
        const bool DefaultPreloadValue = true;
#else
        const bool DefaultPreloadValue = false;
#endif
        public static bool Shops { get; } = DefaultPreloadValue;
        public static bool MonsterSlayer { get; } = DefaultPreloadValue;
        public static bool Quests { get; } = DefaultPreloadValue;
        public static bool SpecialOrders { get; } = DefaultPreloadValue;
    }
}
