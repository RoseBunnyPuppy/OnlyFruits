using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyFruitsMod.Features.Quests
{
    public class NotImplementedSpecialOrderTag : ISpecialOrderTag
    {
        public const string Magic = "NOT_IMPLEMENTED";
        public bool Inverted { get; set; }
        public override string ToString()
        {
            if (this.Inverted) return $"!{Magic}";
            return Magic;
        }
    }
    public class BasicSpecialOrderTag : ISpecialOrderTag
    {
        public bool Inverted { get; set; }
        public string Condition { get; set; }

        public BasicSpecialOrderTag(string condition)
        {
            Condition = condition;
        }

        public override string ToString()
        {
            if (this.Inverted) return $"!{this.Condition}";
            return this.Condition;
        }
    }
    public interface ISpecialOrderTag
    {
        bool Inverted { get; set; }
    }

    public class SpecialOrderRequiredTagsParser
    {
        public static SpecialOrderRequiredTagsParser Instance { get; } = new SpecialOrderRequiredTagsParser();

        private ISpecialOrderTag DeserializeSinglePart(string part)
        {
            var clean = part.Trim();
            if (clean.StartsWith('!'))
            {
                var output = this.DeserializeSinglePart(clean.Substring(1));
                output.Inverted = !output.Inverted;
                return output;
            }
            else if (clean == NotImplementedSpecialOrderTag.Magic) return new NotImplementedSpecialOrderTag();
            return new BasicSpecialOrderTag(clean);
        }

        public IEnumerable<ISpecialOrderTag> Deserialize(string? raw)
        {
            if (raw == null) return Array.Empty<ISpecialOrderTag>();

            var parts = raw.Trim().Split(',');
            return parts.Select(this.DeserializeSinglePart).ToArray();
        }

        public string? Serialize(IEnumerable<ISpecialOrderTag>? tags)
        {
            if (tags == null) return null;
            return string.Join(", ", tags.Select(tag => tag.ToString()));
        }
    }
}
