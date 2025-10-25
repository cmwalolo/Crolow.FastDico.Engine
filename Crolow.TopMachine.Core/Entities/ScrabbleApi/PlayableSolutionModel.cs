using Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi;

namespace Crolow.FastDico.Common.Models.ScrabbleApi.Game;

public class PlayableSolutionModel : IPlayableSolutionModel
{
    public string KpiRate { get; set; }
    public string Position { get; set; }
    public int Points { get; set; }
    public float PlayedTime { get; set; }
    public int TotalPoints { get; set; }
    public float TotalPlayedTime { get; set; }
    public string Rack { get; set; }
    public string Word { get; set; }

    public float RateTime { get; set; }
    public float RateRound { get; set; }
    public float RateAll { get; set; }
}
