namespace RainbowChallengeTracker.DBAccess.Models
{
    internal class ChallengeModel
    {
        public int ID { get; set; }
#pragma warning disable CS8618 
        public string Text { get; set; }
#pragma warning restore CS8618 
        public int Amount { get; set; }
    }
}
