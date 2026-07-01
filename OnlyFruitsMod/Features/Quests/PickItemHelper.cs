namespace OnlyFruitsMod.Features.Quests
{
 

    public class PickItemHelper
    {
        const string PICK_ITEM = "PICK_ITEM";
        public static PickItemHelper Instance { get; } = new PickItemHelper();


        public string Serialize(IEnumerable<string> choices)
        {
            var joined = string.Join(' ', choices);
            if (string.IsNullOrEmpty(joined)) return string.Empty;
            return $"{PICK_ITEM} {joined}";
        }
    }
}
