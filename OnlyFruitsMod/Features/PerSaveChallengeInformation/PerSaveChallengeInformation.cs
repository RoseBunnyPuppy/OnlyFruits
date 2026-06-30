using OnlyFruitsMod.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyFruitsMod.Features.PerSaveChallengeInformation
{
    public class PerSaveChallengeInformation
    {
        /// <summary>
        ///   Whether the challenge is currently active.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        ///   Whether the current save file has ever had the challenge enabled.
        /// </summary>
        public bool EverEnabled { get; set; }
    }
}
