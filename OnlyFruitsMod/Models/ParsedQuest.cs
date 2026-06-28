using System.Diagnostics;

namespace OnlyFruitsMod.Models
{
    public class ParsedQuest
    {
        public string Raw { get; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? Hint { get; set; }
        public string? Requirement { get; set; }
        public string? NextQuests { get; set; }
        public string MoneyReward { get; set; }
        public string? RewardDescription { get; set; }
        public string? CanBeCancelled { get; set; }
        public string? ReactionDialogue { get; set; }

        public ParsedQuest(
            string raw,
            string type,
            string title,
            string description,
            string? hint,
            string? requirement,
            string? nextQuests,
            string moneyReward,
            string? rewardDescription,
            string? canBeCancelled,
            string? reactionDialogue
        )
        {
            Raw = raw;
            Type = type;
            Title = title;
            Description = description;
            Hint = hint;
            Requirement = requirement;
            NextQuests = nextQuests;
            MoneyReward = moneyReward;
            RewardDescription = rewardDescription;
            CanBeCancelled = canBeCancelled;
            ReactionDialogue = reactionDialogue;
        }


        public string Serialize()
        {
            var parts = new string?[]
            {
                this.Type,
                this.Title,
                this.Description,
                this.Hint,
                this.Requirement,
                this.NextQuests,
                this.MoneyReward,
                this.RewardDescription,
                this.CanBeCancelled,
                this.ReactionDialogue,
            };
            var sansTrailingNull = parts.Reverse().SkipWhile(x => x == null).Reverse().ToArray();
            var output = string.Join('/', sansTrailingNull.Select(x => x ?? string.Empty));
            //if (!string.Equals(output, this.Raw))
            //    _ = 23;
            return output;
        }

        public static ParsedQuest Parse(string raw)
        {
            var parts = raw.Split('/');
            string? GetOptionalPart(int index)
            {
                if (parts.Length <= index) return null;
                return parts[index];
            }
            return new ParsedQuest(
                raw,
                parts[0],
                parts[1],
                parts[2],
                GetOptionalPart(3),
                GetOptionalPart(4),
                GetOptionalPart(5),
                parts[6],
                GetOptionalPart(7),
                GetOptionalPart(8),
                GetOptionalPart(9)
            );
        }
    }

}
