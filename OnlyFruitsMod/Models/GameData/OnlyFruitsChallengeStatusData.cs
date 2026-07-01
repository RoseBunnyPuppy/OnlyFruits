namespace OnlyFruitsMod.Models.GameData
{
    /// <summary>
    ///   Information about whether the challenge is enabled in a savefile.
    /// </summary>
    public class OnlyFruitsChallengeStatusData
    {
        public const string DataKey = "ChallengeStatus";
        /// <summary>
        ///  Whether the challenge is enabled.
        /// </summary>
        public bool? IsEnabled { get; set; }
        
    }
}
