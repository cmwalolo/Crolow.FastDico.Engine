using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.Common.Models.ScrabbleApi.Kpi;
using Crolow.FastDico.ScrabbleApi.Extensions;

namespace Crolow.FastDico.ScrabbleApi.Components.Rounds.Evaluators
{
    public class RoundEvaluator
    {
        public static void Evaluate(CurrentGame currentGame, PlayedRounds round, PlayableSolution selectedRound = null)
        {
            KpiRate rateKpi = new KpiRate();

            if (selectedRound.Bonus > 0)
            {
                rateKpi.Set(KpiKeys.Scrabble, true);
                switch (selectedRound.Tiles.Count())
                {
                    case 7:
                        rateKpi.Set(KpiKeys.Scrabble7, true);
                        break;
                    case 8:
                        rateKpi.Set(KpiKeys.Scrabble8, true);
                        break;
                    case 9:
                        rateKpi.Set(KpiKeys.Scrabble9, true);
                        break;
                    case int n when n > 9:
                        rateKpi.Set(KpiKeys.ScrabbleMore, true);
                        break;
                }
            }

            var hooks = EvaluateScoreHook(selectedRound);
            switch (hooks)
            {
                case 2:
                    rateKpi.Set(KpiKeys.HookX2, true);
                    break;
                case 3:
                    rateKpi.Set(KpiKeys.HookX3, true);
                    break;
                case int n when n > 3:
                    rateKpi.Set(KpiKeys.HookMore, true);
                    break;
            }

            var parallels = EvaluateScoreParallel(selectedRound);
            switch (parallels)
            {
                case 2:
                    rateKpi.Set(KpiKeys.ParallelX2, true);
                    break;
                case 3:
                    rateKpi.Set(KpiKeys.ParallelX3, true);
                    break;
                case int n when n > 3:
                    rateKpi.Set(KpiKeys.ParallelMore, true);
                    break;
            }

            rateKpi.Set(KpiKeys.Round, true);

            if (IsJoker(selectedRound))
            {
                rateKpi.Set(KpiKeys.Joker, true);
            }

            if (HasLettersHeight(currentGame, selectedRound, 8, 10))
            {
                rateKpi.Set(KpiKeys.HighLetter, true);
            }

            if (HasLettersHeight(currentGame, selectedRound, 4, 7))
            {
                rateKpi.Set(KpiKeys.HalfLetter, true);
            }

            var len = selectedRound.Tiles.Count;

            switch (len)
            {
                case 2 or 3:
                    rateKpi.Set(KpiKeys.ShortWord, true);
                    break;
                case >= 4 and <= 6:
                    rateKpi.Set(KpiKeys.AverageWord, true);
                    break;
                case >= 7 and <= 8:
                    rateKpi.Set(KpiKeys.NormalWord, true);
                    break;
                case > 8:
                    rateKpi.Set(KpiKeys.LongWord, true);
                    break;
            }

            var bagLetters = selectedRound.Rack.Tiles.Where(p => p.Points > 2 && !p.IsJoker).GroupBy(p => p.Letter);
            foreach (var g in bagLetters)
            {
                rateKpi.Set(65 + g.Key, true);
            }

            var solLetters = selectedRound.Tiles.Where(p => p.Points > 2 && p.Parent.Status == 1 && !p.IsJoker).GroupBy(p => p.Letter);
            foreach (var g in solLetters)
            {
                rateKpi.Set(97 + g.Key, true);
            }

            selectedRound.KpiRate = rateKpi.Value.ToString();
        }

        private static bool HasLettersHeight(CurrentGame game, PlayableSolution round, int min, int max)
        {
            return round.Tiles.Any(p =>
            {
                var pts = game.ControllersSetup.DictionaryContainer.TileConfiguration.LettersByByte[p.Letter].Points;
                return p.Parent.Status == -1
                        && !p.IsJoker
                        && pts >= min && pts <= max;
            });
        }

        private static bool IsJoker(PlayableSolution round)
        {
            return round.Tiles.Any(p => p.IsJoker && p.Parent.Status == -1);
        }

        private static int EvaluateScoreHook(PlayableSolution round)
        {
            return (round.Tiles.Count(p => p.Parent.Status == 1));
        }

        private static int EvaluateScoreParallel(PlayableSolution round)
        {
            int count = 0; // gc.GameBoard.evalRaccords(tround);
            foreach (var l in round.Tiles)
            {
                if (l.Parent.Status == -1)
                {
                    int c = l.Parent.GetPivotLetters(round.Position.Direction);
                    if (c > 0)
                    {
                        count++;
                    }
                }
            }

            return count;

        }
    }
}
