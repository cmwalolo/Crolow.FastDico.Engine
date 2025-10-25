namespace Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi
{
    public interface IPlayableSolutionModel
    {
        float PlayedTime { get; set; }
        int Points { get; set; }
        string Position { get; set; }
        string Rack { get; set; }
        float TotalPlayedTime { get; set; }
        int TotalPoints { get; set; }
        string Word { get; set; }

        public float RateTime { get; set; }
        public float RateRound { get; set; }
        public float RateAll { get; set; }
        public string KpiRate { get; set; }
    }
}