using Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi;

namespace Crolow.FastDico.Common.Models.ScrabbleApi.Game
{
    public class EvaluatorConfig : IEvaluatorConfig
    {
        // --- Score ratios ---
        public float SousTopRatioScrabbleDiv { get; set; } = 50;
        public float SousTopRatioDiv { get; set; } = 20;
        public float ScoreMotRatioMul { get; set; } = 1;
        public float RackRatioMult { get; set; } = 1;
        public float ScrabbleRatioDiv { get; set; } = 25;
        public float RaccordsRatioMul { get; set; } = 1;
        public float CollageRatioMul { get; set; } = 1.5f;
        public float AppuiRatioMul { get; set; } = 15f;

        // --- Frequencies ---
        public int ScrabbleFrequence { get; set; } = 100;
        public int AppuisFrequence { get; set; } = 80;
        public int CollagesFrequence { get; set; } = 80;
        public int RaccordsFrequence { get; set; } = 80;
        public int RackFrequence { get; set; } = 40;
        public int BoostFrequence { get; set; } = 80;
        public int SkipFrequence { get; set; } = 10;
        public int StartBoostRound { get; set; } = 2;
        public int StartBoostFrequence { get; set; } = 30;


    }
}
