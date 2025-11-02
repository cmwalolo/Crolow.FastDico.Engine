using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi;
using Crolow.FastDico.GadDag;
using Crolow.FastDico.ScrabbleApi.Extensions;
using System.Collections;

namespace Crolow.FastDico.ScrabbleApi.Components.Rounds.Evaluators
{

    public class Evaluator
    {
        private readonly CurrentGame currentGame;
        private readonly IEvaluatorConfig config;

        public class RoundsComparer : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                return ((RatingRound)x).scoreAll < ((RatingRound)y).scoreAll ? 0 : 1;
            }
        }

        public bool DoCollages = false;
        public bool DoScrabble = false;
        public bool DoAppuis = false;
        public bool DoRaccords = false;
        public bool DoRack = false;
        public bool DoBoost = false;
        public bool DoSkip = false;

        public class RatingRound
        {
            public bool NoResults = false;
            public bool Valid = true;
            public bool rejet = false;
            public int nbSolutions = 0;
            public int nbTirages = 0;
            public float scoreAll = 0;
            public float scorerack = 1;
            public float scoresoustop = 0;
            public float scorescrabble = 0;
            public float scoreraccords = 0;
            public float scorecollage = 0;
            public float scorecollagemots = 0;
            public float scoremot = 0;
            public float scoreappui = 0;
        }

        public int[] Tiles_points = new int[]
        {
            /* x A B C D  E F G H I J  K L M N O P Q R S T U V  W  X  Y  Z ? */
            0,1,3,3,2, 1,4,2,4,1,8,10,1,2,1,1,3,8,1,1,1,1,4,10,10,10,10,0
        };

        public Evaluator(CurrentGame currentGame)
        {
            this.currentGame = currentGame;
            this.config = currentGame.GameObjects.GameConfig.BoostConfig;

            if (this.config == null)
            {
                this.config = new EvaluatorConfig();
                if (currentGame.GameObjects.GameConfig.JokerMode)
                {
                    this.config.ScrabbleFrequence = 85;
                    this.config.CollagesFrequence = 60;
                    this.config.AppuisFrequence = 90;
                    this.config.RaccordsFrequence = 70;
                    this.config.RackFrequence = 40;
                    this.config.BoostFrequence = 80;
                    this.config.SkipFrequence = 15;
                    this.config.StartBoostRound = 2;
                    this.config.StartBoostFrequence = 50;
                }
                else
                {
                    this.config.ScrabbleFrequence = 70;
                    this.config.CollagesFrequence = 60;
                    this.config.AppuisFrequence = 100;
                    this.config.RaccordsFrequence = 70;
                    this.config.RackFrequence = 80;
                    this.config.BoostFrequence = 80;
                    this.config.StartBoostRound = 2;
                    this.config.SkipFrequence = 10;
                    this.config.StartBoostFrequence = 30;
                }

                currentGame.GameObjects.GameConfig.BoostConfig = this.config;
            }
        }

        public bool IsBoosted() => DoBoost;
        public void BoostedOff() => DoBoost = false;

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

            c = Random.Shared.Next(100);
            if (c < config.ScrabbleFrequence)
                DoScrabble = true;

            c = Random.Shared.Next(100);
            if (c < config.AppuisFrequence)
                DoAppuis = true;

            c = Random.Shared.Next(100);
            if (c < config.RaccordsFrequence)
                DoRaccords = true;

            c = Random.Shared.Next(100);
            if (c < config.RackFrequence)
                DoRack = true;
        }

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

                if (!DoBoost)
                {
                    if (DoScrabble)
                        EvaluateScrabbleSousTop(rate, selectedRound, word, round.SubTops.FirstOrDefault());
                    else
                        EvaluateSousTop(rate, selectedRound, word, round.SubTops.FirstOrDefault());
                }

                EvaluateScoreMot(rate, selectedRound, word);

                if (DoRaccords)
                    EvaluateRaccords(rate, selectedRound, word);

                if (DoRack)
                    EvaluateRack(rate, selectedRound, word, round.SubTops.FirstOrDefault());
            }

            rate.scoreAll /= Math.Max(1, rate.nbSolutions);
            return rate;
        }

        private void EvaluateNumberOfSolutions(RatingRound rate, PlayedRounds round)
        {
            rate.nbSolutions = round.Tops.Count;
        }

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

        private void EvaluateScoreAppui(RatingRound rate, PlayableSolution round, string word)
        {
            if (round.Tiles.Count(p => p.Parent.Status != 1) < 5)
                return;

            int c = round.Tiles.Count(t => t.Parent.Status == 1);

            if (c > 1)
            {
                rate.scoreappui = c;
                rate.scoreAll += rate.scoreappui * config.AppuiRatioMul;
            }
        }

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

        private void EvaluateRaccords(RatingRound rate, PlayableSolution round, string word)
        {
            if (word.Length > 4)
            {
                int count = CompteRaccord(word);
                rate.scoreraccords = count;
                rate.scoreAll += count * config.RaccordsRatioMul;
            }
        }

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

        private void EvaluateScrabbleSousTop(RatingRound rate, PlayableSolution round, string word, PlayableSolution subTop)
        {
            if (round.Bonus > 0)
            {
                float diff = 100 - subTop.Points / (float)round.Points * 100;
                rate.scoresoustop = diff / config.SousTopRatioScrabbleDiv;
                rate.scoreAll += rate.scoresoustop;
            }
        }

        private void EvaluateSousTop(RatingRound rate, PlayableSolution round, string word, PlayableSolution subTop)
        {
            float diff = 100 - subTop.Points / (float)round.Points * 100;
            rate.scoresoustop = diff / config.SousTopRatioDiv;
            rate.scoreAll += rate.scoresoustop;
        }

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
