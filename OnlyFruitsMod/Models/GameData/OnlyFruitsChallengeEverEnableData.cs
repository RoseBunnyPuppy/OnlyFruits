namespace OnlyFruitsMod.Models.GameData
{
    /// <summary>
    ///   Information about whether the challenge was ever enabled in a savefile.
    /// </summary>
    public class OnlyFruitsChallengeEverEnableData
    {
        public const string DataKey = "EverChallenge";
        
        public bool? Status { get; set; }

    }
}
