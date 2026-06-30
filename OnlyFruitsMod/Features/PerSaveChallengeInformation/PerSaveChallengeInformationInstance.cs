using OnlyFruitsMod.Models.GameData;
using StardewModdingAPI;

namespace OnlyFruitsMod.Features.PerSaveChallengeInformation
{
    public class PerSaveChallengeInformationInstance
    {
        const bool DefaultEverEnabled = false;
        private readonly IModHelper helper;

        /// <summary>
        ///   The current information.  Will be null if not in a save file.
        /// </summary>
        public PerSaveChallengeInformation? Information { get; private set; }
        


        /// <summary>
        ///   Indicates whether the current save has been fully loaded.
        /// </summary>
        public bool HasPerSaveLoaded => this.Information != null;

        /// <summary>
        ///     Indicates whether the challenge is currently enabled for the save.  If
        ///   no save is loaded, also returns false.
        /// </summary>
        public bool IsChallengeEnabled => this.Information?.IsEnabled ?? false;


        public PerSaveChallengeInformationInstance(
            IModHelper helper
        )
        {
            this.helper = helper;
        }

        public PerSaveChallengeInformation GetOrCreateChallengeInformation()
        {
            var output = this.Information;
            if (output != null) return output;
            output = new PerSaveChallengeInformation();
            this.Information = output;
            return output;
        }
        
        public void UnsetPerSaveInfo()
        {
            this.Information = null;
        }
    }
}
