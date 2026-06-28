namespace OnlyFruitsMod.Features.UIHelpers
{

    public static class EnumChoiceMap
    {
        public static EnumChoiceMap<TEnum> Create<TEnum>(
            Dictionary<TEnum, string> choiceMap,
            TEnum defaultValue,
            TEnum[] orderedChoices,
            Dictionary<string, TEnum>? reverseMap = null
        )
            where TEnum : Enum
        {
            return new EnumChoiceMap<TEnum>(choiceMap, defaultValue, orderedChoices, reverseMap);
        }
    }
}
