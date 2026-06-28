using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyFruitsMod.Features.Quests.Models
{
    public class SpecialOrderKeysConfigModel
    {
        public HashSet<string> AlwaysResetMoney { get; set; } = new HashSet<string>();
        public HashSet<string> AlwaysDisabledQi { get; set; } = new HashSet<string>();
        public HashSet<string> PotentiallyDisabledQi { get; set; } = new HashSet<string>();


        public bool IsAlwaysMoneyReset(string key)
        {
            return this.AlwaysResetMoney.Contains(key);
        }

        public bool IsDisabledQi(string key)
        {
            return this.AlwaysDisabledQi.Contains(key) || this.PotentiallyDisabledQi.Contains(key);
        }
    }

}
