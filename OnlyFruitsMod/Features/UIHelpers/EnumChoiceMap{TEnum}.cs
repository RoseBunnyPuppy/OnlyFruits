namespace OnlyFruitsMod.Features.UIHelpers
{
    public class EnumChoiceMap<TEnum>
        where TEnum : Enum
    {
        private readonly Dictionary<TEnum, string> choiceMap;
        private readonly TEnum defaultValue;
        private readonly TEnum[] orderedChoices;
        private readonly Dictionary<string, TEnum> reverseMap;

        public EnumChoiceMap(
            Dictionary<TEnum, string> choiceMap,
            TEnum defaultValue,
            TEnum[] orderedChoices,
            Dictionary<string, TEnum>? reverseMap = null
        )
        {
            this.choiceMap = new Dictionary<TEnum, string>(choiceMap);
            this.defaultValue = defaultValue;
            this.orderedChoices = orderedChoices.ToArray();
            this.reverseMap = BuildReverseLookup(choiceMap, orderedChoices, reverseMap);
        }

        private static Dictionary<string, TEnum> BuildReverseLookup(
            Dictionary<TEnum, string> choiceMap,
            TEnum[] orderedChoices,
            Dictionary<string, TEnum>? reverseMap
        )
        {
            if (reverseMap != null) return new Dictionary<string, TEnum>(reverseMap);
            var output = new Dictionary<string, TEnum>();
            foreach (var choice in orderedChoices)
            {
                output[choiceMap[choice]] = choice;
            }
            return output;
        }


        public string GetStringValue(TEnum current, TEnum fallback)
        {
            if (this.choiceMap.TryGetValue(current, out var strValue)) return strValue;
            return this.choiceMap[fallback];
        }


        public string[] GetAllowed() => this.orderedChoices.Select(choice => this.GetStringValue(choice)).ToArray();

        public string GetStringValue(TEnum current) => this.GetStringValue(current, this.defaultValue);


        public TEnum GetEnumValue(string strValue)
        {
            if (this.reverseMap.TryGetValue(strValue, out var match)) return match;
            return this.defaultValue;
        }
    }
}
