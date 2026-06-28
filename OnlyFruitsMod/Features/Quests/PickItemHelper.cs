namespace OnlyFruitsMod.Features.Quests
{
 

    public class PickItemHelper
    {
        const string PICK_ITEM = "PICK_ITEM";
        public static PickItemHelper Instance { get; } = new PickItemHelper();
        public IEnumerable<string> Deserialize(string raw)
        {
            var parts = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return Array.Empty<string>();
            if (parts[0] != PICK_ITEM) throw new ArgumentException($"Must start with '{PICK_ITEM}': Actual '{parts[0]}'", nameof(raw));
            return parts.Skip(1);
        }

        public string Serialize(IEnumerable<string> choices)
        {
            var joined = string.Join(' ', choices);
            if (string.IsNullOrEmpty(joined)) return string.Empty;
            return $"{PICK_ITEM} {joined}";
        }
    }
}
