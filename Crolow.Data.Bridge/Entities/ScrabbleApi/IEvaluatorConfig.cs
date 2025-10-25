namespace Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi
{
    public interface IEvaluatorConfig
    {
        float AppuiRatioMul { get; set; }
        int AppuisFrequence { get; set; }
        int BoostFrequence { get; set; }
        float CollageRatioMul { get; set; }
        int CollagesFrequence { get; set; }
        int RaccordsFrequence { get; set; }
        float RaccordsRatioMul { get; set; }
        int RackFrequence { get; set; }
        float RackRatioMult { get; set; }
        float ScoreMotRatioMul { get; set; }
        int ScrabbleFrequence { get; set; }
        float ScrabbleRatioDiv { get; set; }
        int SkipFrequence { get; set; }
        float SousTopRatioDiv { get; set; }
        float SousTopRatioScrabbleDiv { get; set; }
        int StartBoostFrequence { get; set; }
        int StartBoostRound { get; set; }
    }
}