using OnlyFruitsMod.Features.Quests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyFruitsMod.Features.Quests
{


    public static class DefaultSpecialOrderKeysProvider
    {
        public static SpecialOrderKeysConfigModel Create()
        {
            return new SpecialOrderKeysConfigModel
            {
                AlwaysDisabledQi = new HashSet<string>
                {
                    "QiChallenge3",
                    "QiChallenge4",
                    "QiChallenge5",
                    "QiChallenge8",
                    "QiChallenge9",
                    "QiChallenge10",
                    "QiChallenge12",
                },
                PotentiallyDisabledQi = new HashSet<string>
                {
                    "QiChallenge6",
                    "QiChallenge7",
                },
                AlwaysResetMoney = new HashSet<string>
                {
                    "Willy",
                    "Clint",
                    "Pierre",
                    "Robin",
                    "Emily",
                    "Demetrius",
                    "Demetrius2",
                    "Gus",
                    "Wizard",
                    "Evelyn",
                    "Linus",
                    "Wizard2",
                    "Gunther",
                    "Robin2",
                    "DesertFestivalMarlon1",
                    "DesertFestivalMarlon2",
                    "DesertFestivalMarlon3",
                    "Willy2",
                }
            };
        }
    }
}