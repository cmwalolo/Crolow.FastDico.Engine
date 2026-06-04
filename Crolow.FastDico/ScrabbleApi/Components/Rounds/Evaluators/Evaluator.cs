using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi;
using Crolow.FastDico.GadDag;
using Crolow.FastDico.ScrabbleApi.Extensions;
using System.Collections;

namespace Crolow.FastDico.ScrabbleApi.Components.Rounds.Evaluators
{

    /// <summary>
    /// Rates candidate rounds according to configurable boost, rack, scrabble, hook, parallel, and word-quality criteria.
    /// </summary>
    public class Evaluator
    {
        private readonly CurrentGame currentGame;
        private readonly IEvaluatorConfig config;

        /// <summary>
        /// Compares rated rounds by their overall score.
        /// </summary>
        public class RoundsComparer : IComparer
        {
            /// <summary>
            /// Compares two rating rounds.
            /// </summary>
            /// <param name="x">First rating round.</param>
            /// <param name="y">Second rating round.</param>
            /// <returns>A comparison value based on the overall score.</returns>
            int IComparer.Compare(object x, object y)
            {
                return ((RatingRound)x).scoreAll < ((RatingRound)y).scoreAll ? 0 : 1;
            }
        }

        /// <summary>
        /// Indicates whether collage scoring should be applied.
        /// </summary>
        public bool DoCollages = false;

        /// <summary>
        /// Indicates whether scrabble bonus scoring should be encouraged.
        /// </summary>
        public bool DoScrabble = false;

        /// <summary>
        /// Indicates whether hook support scoring should be applied.
        /// </summary>
        public bool DoAppuis = false;

        /// <summary>
        /// Indicates whether raccord scoring should be applied.
        /// </summary>
        public bool DoRaccords = false;

        /// <summary>
        /// Indicates whether rack-quality scoring should be applied.
        /// </summary>
        public bool DoRack = false;

        /// <summary>
        /// Indicates whether boosted selection is active.
        /// </summary>
        public bool DoBoost = false;

        /// <summary>
        /// Indicates whether rating should be skipped for the current round.
        /// </summary>
        public bool DoSkip = false;

        /// <summary>
        /// Holds individual score components for a rated round.
        /// </summary>
        public class RatingRound
        {
            /// <summary>
            /// Indicates whether the solve produced no results.
            /// </summary>
            public bool NoResults = false;

            /// <summary>
            /// Indicates whether the rated round is valid.
            /// </summary>
            public bool Valid = true;

            /// <summary>
            /// Indicates whether the rated rack should be rejected.
            /// </summary>
            public bool rejet = false;

            /// <summary>
            /// Gets the number of top solutions in the solve result.
            /// </summary>
            public int nbSolutions = 0;

            /// <summary>
            /// Gets the number of draws evaluated.
            /// </summary>
            public int nbTirages = 0;

            /// <summary>
            /// Gets the normalized overall score.
            /// </summary>
            public float scoreAll = 0;

            /// <summary>
            /// Gets the rack-quality score component.
            /// </summary>
            public float scorerack = 1;

            /// <summary>
            /// Gets the sub-top score component.
            /// </summary>
            public float scoresoustop = 0;

            /// <summary>
            /// Gets the scrabble bonus score component.
            /// </summary>
            public float scorescrabble = 0;

            /// <summary>
            /// Gets the raccord score component.
            /// </summary>
            public float scoreraccords = 0;

            /// <summary>
            /// Gets the collage score component.
            /// </summary>
            public float scorecollage = 0;

            /// <summary>
            /// Gets the collage word-count score component.
            /// </summary>
            public float scorecollagemots = 0;

            /// <summary>
            /// Gets the word-length score component.
            /// </summary>
            public float scoremot = 0;

            /// <summary>
            /// Gets the support-hook score component.
            /// </summary>
            public float scoreappui = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Evaluator"/> class.
        /// </summary>
        /// <param name="currentGame">Current game context that supplies configuration and dictionary services.</param>
        public Evaluator(CurrentGame currentGame)
        {
            this.currentGame = currentGame;
            this.config = currentGame.GameObjects.GameConfig.BoostConfig;

            if (true || this.config == null)
            {
                this.config = new EvaluatorConfig();
                if (currentGame.GameObjects.GameConfig.JokerMode)
                {
                    this.config.ScrabbleFrequence = 75;
                    this.config.CollagesFrequence = 40;
                    this.config.AppuisFrequence = 90;
                    this.config.RaccordsFrequence = 60;
                    this.config.RackFrequence = 40;
                    this.config.BoostFrequence = 90;
                    this.config.SkipFrequence = 15;
                    this.config.StartBoostRound = 2;
                    this.config.StartBoostFrequence = 30;
                }
                else
                {
                    this.config.ScrabbleFrequence = 70;
                    this.config.CollagesFrequence = 40;
                    this.config.AppuisFrequence = 100;
                    this.config.RaccordsFrequence = 60;
                    this.config.RackFrequence = 40;
                    this.config.BoostFrequence = 90;
                    this.config.StartBoostRound = 2;
                    this.config.SkipFrequence = 10;
                    this.config.StartBoostFrequence = 30;
                }

                currentGame.GameObjects.GameConfig.BoostConfig = this.config;
            }
        }

        /// <summary>
        /// Gets a value indicating whether boosted selection is active.
        /// </summary>
        /// <returns><c>true</c> when boosted selection is active.</returns>
        public bool IsBoosted() => DoBoost;

        /// <summary>
        /// Disables boosted selection for the current evaluation cycle.
        /// </summary>
        public void BoostedOff() => DoBoost = false;

        /// <summary>
        /// Initializes random evaluation switches for the current round.
        /// </summary>
        public void Initialize()
        {
            DoCollages = DoScrabble = DoAppuis = false;
            DoSkip = DoRaccords = DoRack = DoBoost = false;

            int c = Random.Shared.Next(100);
            if (currentGame.GameObjects.Round > 0)
            {
                if (c < config.BoostFrequence && currentGame.GameObjects.Round > config.StartBoostRound)
                    DoBoost = true;

                if (c < config.StartBoostFrequence && currentGame.GameObjects.Round <= config.StartBoostRound)
                    DoBoost = true;
            }

            c = Random.Shared.Next(100);

            if (c < config.SkipFrequence && !DoBoost)
            {
                DoSkip = true;
                return;
            }

            if (currentGame.GameObjects.Round < 4)
                c = (int)(1.5f * c);

            if (c < config.CollagesFrequence)
                DoCollages = true;

            //            c = Random.Shared.Next(100);

            if (DoBoost && c < config.ScrabbleFrequence)
                DoScrabble = true;

            //            c = Random.Shared.Next(100);
            if (c < config.AppuisFrequence)
                DoAppuis = true;

            //            c = Random.Shared.Next(100);
            if (c < config.RaccordsFrequence)
                DoRaccords = true;

            //            c = Random.Shared.Next(100);
            if (c < config.RackFrequence)
                DoRack = true;
        }

        /// <summary>
        /// Evaluates a played-rounds collection or a selected round and returns its rating details.
        /// </summary>
        /// <param name="round">Played-rounds collection to evaluate.</param>
        /// <param name="selectedRound">Optional selected round; defaults to the first top solution.</param>
        /// <returns>The rating result for the selected round.</returns>
        public RatingRound Evaluate(PlayedRounds round, PlayableSolution selectedRound = null)
        {
            RatingRound rate = new();

            selectedRound ??= round.Tops.FirstOrDefault();
            if (selectedRound != null)
            {
                string word = selectedRound.GetWord(currentGame.ControllersSetup.DictionaryContainer.TilesUtils);

                EvaluateNumberOfSolutions(rate, round);

                if (DoSkip)
                    return rate;

                if (DoCollages)
                    EvaluateCollages(rate, selectedRound, word);

                if (DoAppuis || DoBoost)
                    EvaluateScoreAppui(rate, selectedRound, word);

                EvaluateScrabble(rate, selectedRound, word, DoScrabble);

                //if (!DoBoost)
                //{
                //    if (DoScrabble)
                //        EvaluateScrabbleSousTop(rate, selectedRound, word, round.SubTops.FirstOrDefault());
                //    else
                //        EvaluateSousTop(rate, selectedRound, word, round.SubTops.FirstOrDefault());
                //}

                EvaluateScoreMot(rate, selectedRound, word);

                if (DoRaccords)
                    EvaluateRaccords(rate, selectedRound, word);

                if (DoRack)
                    EvaluateRack(rate, selectedRound, word, round.SubTops.FirstOrDefault());
            }

            rate.scoreAll /= Math.Max(1, rate.nbSolutions);
            return rate;
        }

        /// <summary>
        /// Records the number of top solutions in the rating.
        /// </summary>
        /// <param name="rate">Rating to update.</param>
        /// <param name="round">Played-rounds collection being evaluated.</param>
        private void EvaluateNumberOfSolutions(RatingRound rate, PlayedRounds round)
        {
            rate.nbSolutions = round.Tops.Count;
        }

        /// <summary>
        /// Scores the candidate word based on length and minimum newly placed letters.
        /// </summary>
        /// <param name="rate">Rating to update.</param>
        /// <param name="round">Round being evaluated.</param>
        /// <param name="word">Display word for the round.</param>
        private void EvaluateScoreMot(RatingRound rate, PlayableSolution round, string word)
        {
            if (word.Length > 9)
                rate.scoremot = 2;
            else if (word.Length > 7)
                rate.scoremot = 1.5f;
            else
                rate.scoremot = 1;

            if (round.Tiles.Count(p => p.Parent.Status != 1) < 3)
            {
                rate.scoremot = -15;
                rate.scoreAll = -15;
            }
            else
            {
                rate.scoreAll += rate.scoremot * config.ScoreMotRatioMul;
            }
        }

        /// <summary>
        /// Scores the support created by existing board tiles used in the candidate word.
        /// </summary>
        /// <param name="rate">Rating to update.</param>
        /// <param name="round">Round being evaluated.</param>
        /// <param name="word">Display word for the round.</param>
        private void EvaluateScoreAppui(RatingRound rate, PlayableSolution round, string word)
        {
            if (round.Tiles.Count(p => p.Parent.Status != 1) < 5)
                return;

            int c = round.Tiles.Count(t => t.Parent.Status == 1);

            int runs = round.Tiles
                .Select((t, i) => new { Tile = t, Index = i })
                .Count(x =>
                    x.Tile.Parent.Status == 1 &&
                    (x.Index == 0 || round.Tiles[x.Index - 1].Parent.Status != 1));

            if (c > 1)
            {
                rate.scoreappui = c*runs;
                rate.scoreAll += rate.scoreappui * config.AppuiRatioMul;
            }
        }

        /// <summary>
        /// Scores parallel-word collage opportunities created by newly placed tiles.
        /// </summary>
        /// <param name="rate">Rating to update.</param>
        /// <param name="round">Round being evaluated.</param>
        /// <param name="word">Display word for the round.</param>
        private void EvaluateCollages(RatingRound rate, PlayableSolution round, string word)
        {
            if (word.Length < 5)
                return;

            int count = 0;
            int words = 0;
            foreach (var l in round.Tiles)
            {
                if (l.Parent.Status == -1)
                {
                    int c = l.Parent.GetPivotLetters(round.Position.Direction);
                    if (c > 0)
                    {
                        count += Math.Min(3, c);
                        words++;
                    }
                }
            }

            if (count > 1 && words > 2)
            {
                rate.scorecollage = count;
                rate.scorecollagemots = words;
                rate.scoreAll += rate.scorecollage * rate.scorecollagemots * config.CollageRatioMul;
            }
        }

        /// <summary>
        /// Scores raccord opportunities for extending the played word.
        /// </summary>
        /// <param name="rate">Rating to update.</param>
        /// <param name="round">Round being evaluated.</param>
        /// <param name="word">Display word for the round.</param>
        private void EvaluateRaccords(RatingRound rate, PlayableSolution round, string word)
        {
            if (word.Length > 4)
            {
                int count = CompteRaccord(word);
                rate.scoreraccords = count;
                rate.scoreAll += count * config.RaccordsRatioMul;
            }
        }

        /// <summary>
        /// Penalizes candidate rounds that leave a weak rack behind.
        /// </summary>
        /// <param name="rate">Rating to update.</param>
        /// <param name="round">Round being evaluated.</param>
        /// <param name="word">Display word for the round.</param>
        /// <param name="subTop">Best sub-top solution used as a comparison baseline.</param>
        private void EvaluateRack(RatingRound rate, PlayableSolution round, string word, PlayableSolution subTop)
        {
            rate.scorerack = 1;
            var totalLetters = round.Tiles.Count(p => p.Parent.Status == -1);
            var totalJoker = round.Tiles.Count(p => p.Parent.Status == -1 && p.IsJoker);
            var totalPoints = round.Tiles.Where(p => p.Parent.Status == -1).Sum(p => p.Points);

            if (totalJoker > 0 && totalPoints < round.Tiles.Count + 2)
            {
                if (round.Points < subTop.Points + 5)
                {
                    rate.scorerack = -1;
                    rate.scoreAll /= 3;
                }
                else
                {
                    rate.scorerack = -1;
                    rate.scoreAll /= 2;
                }
            }
            else if (totalJoker > 1 || totalPoints < totalLetters + 2)
            {
                rate.scorerack = -1;
                rate.scoreAll /= 3;
            }
        }

        /// <summary>
        /// Scores or penalizes scrabble bonus usage depending on the current evaluation goal.
        /// </summary>
        /// <param name="rate">Rating to update.</param>
        /// <param name="round">Round being evaluated.</param>
        /// <param name="word">Display word for the round.</param>
        /// <param name="doScrabble">Indicates whether scrabble bonuses are encouraged.</param>
        private void EvaluateScrabble(RatingRound rate, PlayableSolution round, string word, bool doScrabble)
        {
            if (round.Bonus > 0)
            {
                if (doScrabble)
                {
                    rate.scorescrabble = round.Bonus / config.ScrabbleRatioDiv;
                    rate.scoreAll += rate.scorescrabble;
                }
                else
                {
                    rate.scorescrabble = -round.Bonus / config.ScrabbleRatioDiv;
                    rate.scoreAll -= 25;
                }
            }
        }

        /// <summary>
        /// Scores the gap between a scrabble top and its sub-top.
        /// </summary>
        /// <param name="rate">Rating to update.</param>
        /// <param name="round">Round being evaluated.</param>
        /// <param name="word">Display word for the round.</param>
        /// <param name="subTop">Sub-top solution used as a baseline.</param>
        private void EvaluateScrabbleSousTop(RatingRound rate, PlayableSolution round, string word, PlayableSolution subTop)
        {
            if (round.Bonus > 0)
            {
                float diff = 100 - subTop.Points / (float)round.Points * 100;
                rate.scoresoustop = diff / config.SousTopRatioScrabbleDiv;
                rate.scoreAll += rate.scoresoustop;
            }
        }

        /// <summary>
        /// Scores the gap between a top and its sub-top.
        /// </summary>
        /// <param name="rate">Rating to update.</param>
        /// <param name="round">Round being evaluated.</param>
        /// <param name="word">Display word for the round.</param>
        /// <param name="subTop">Sub-top solution used as a baseline.</param>
        private void EvaluateSousTop(RatingRound rate, PlayableSolution round, string word, PlayableSolution subTop)
        {
            float diff = 100 - subTop.Points / (float)round.Points * 100;
            rate.scoresoustop = diff / config.SousTopRatioDiv;
            rate.scoreAll += rate.scoresoustop;
        }

        /// <summary>
        /// Counts possible one-letter raccords before and after a word.
        /// </summary>
        /// <param name="word">Word to test for raccord opportunities.</param>
        /// <returns>The number of prefix and suffix raccord matches.</returns>
        private int CompteRaccord(string word)
        {
            GadDagSearch search = new(
                currentGame.ControllersSetup.DictionaryContainer.Dico.Root,
                currentGame.ControllersSetup.DictionaryContainer.TilesUtils);

            var res = search.SearchByPrefix(word, 1);
            res.AddRange(search.SearchBySuffix(word, 1));
            return res.Count;
        }
    }
}
