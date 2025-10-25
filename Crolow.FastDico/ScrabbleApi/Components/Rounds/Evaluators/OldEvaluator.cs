using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.GadDag;
using Crolow.FastDico.ScrabbleApi.Extensions;
using System.Collections;



namespace Crolow.FastDico.ScrabbleApi.Components.Rounds.Evaluators
{
    public class OldEvaluator
    {
        private CurrentGame currentGame;
        public class RoundsComparer : IComparer
        {
            // Calls CaseInsensitiveComparer.Compare with the parameters reversed.
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


        public const float SousTopRatioScrabbleDiv = 50;
        public const float sousTopRatioDiv = 20;
        public const float ScoreMotRatioMul = 1;

        public const float RackRatioMult = 1;           // Multiplier
        public const float ScrabbleRatioDiv = 25;
        public const float RaccordsRatioMul = 1;
        public const float CollageRatioMul = 1.5f;
        public const float AppuiRatioMul = 15f;

        public int ScrabbleFrequence = 60;
        public int AppuisFrequence = 80;
        public int CollagesFrequence = 80;
        public int RaccordsFrequence = 80;
        public int RackFrequence = 40;
        public int BoostFrequence = -1;
        public int SkipFrequence = 10;
        public int StartBoostRound = 2;
        public int StartBoostFrequence = 30;

        private int maxTurn = 30;

        public OldEvaluator(CurrentGame currentGame)

        {
            this.currentGame = currentGame;
            Random rnd = new Random();

            if (currentGame.GameObjects.GameConfig.JokerMode)
            {
                ScrabbleFrequence = 85; ;
                CollagesFrequence = 60;  //cfg.Config.intCollagesFrequence;
                AppuisFrequence = 90;  //cfg.Config.intAppuisFrequence;
                RaccordsFrequence = 70; //  cfg.Config.intRaccordsFrequence;
                RackFrequence = 40; //  cfg.Config.intRackFrequence; 
                BoostFrequence = 80;
                SkipFrequence = 15;
                StartBoostRound = 2;
                StartBoostFrequence = 50;
            }
            else
            {
                ScrabbleFrequence = 70;  //cfg.Config.intScrabbleFrequence;
                CollagesFrequence = 60;  //cfg.Config.intCollagesFrequence;
                AppuisFrequence = 100;  //cfg.Config.intAppuisFrequence;
                RaccordsFrequence = 70; //  cfg.Config.intRaccordsFrequence;
                RackFrequence = 80; //  cfg.Config.intRackFrequence; 
                BoostFrequence = 80;
                StartBoostRound = 2;
                SkipFrequence = 10;
                StartBoostFrequence = 30;
            }

        }

        public bool IsBoosted()
        {
            return DoBoost;
        }

        public void BoostedOff()
        {
            DoBoost = false;
        }

        public void Initialize()
        {

            DoCollages = DoScrabble = DoAppuis = false;
            DoSkip = DoRaccords = DoRack = DoBoost = false;

            int c = Random.Shared.Next(100);
            if (currentGame.GameObjects.Round > 0)
            {

                if (c < BoostFrequence && currentGame.GameObjects.Round > StartBoostRound)
                {
                    DoBoost = true;
                }

                if (c < StartBoostFrequence && currentGame.GameObjects.Round <= StartBoostRound)
                {
                    DoBoost = true;
                }

            }

            c = Random.Shared.Next(100);

            if (c < SkipFrequence && !DoBoost)
            {
                DoSkip = true;
                return;
            }

            if (currentGame.GameObjects.Round < 4)
            {
                c = (int)1.5f * c;
            }
            if (c < CollagesFrequence)
            {
                DoCollages = true;
            }


            c = Random.Shared.Next(100);

            if (c < ScrabbleFrequence)
            {
                DoScrabble = true;
            }

            c = Random.Shared.Next(100);
            if (c < AppuisFrequence)
            {
                DoAppuis = true;
            }

            c = Random.Shared.Next(100);
            if (c < RaccordsFrequence)
            {
                DoRaccords = true;
            }

            c = Random.Shared.Next(100);
            if (c < RackFrequence)
            {
                DoRack = true;
            }

        }

        public RatingRound Evaluate(PlayedRounds round, PlayableSolution selectedRound = null)
        {
            float maxScore = 0;
            int maxItem = 0;
            int item = 0;

            RatingRound rate = new();


            selectedRound = selectedRound ?? round.Tops.FirstOrDefault();
            if (selectedRound != null)
            {
                string word = selectedRound.GetWord(currentGame.ControllersSetup.DictionaryContainer.TilesUtils);

                EvaluateNumberOfSolutions(rate, round);

                if (DoSkip)
                {
                    return rate;
                }

                if (DoCollages)
                {
                    // EvaluateCollagesMots(rate, selectedRound, word);
                    EvaluateCollages(rate, selectedRound, word);
                }

                if (DoAppuis || DoBoost)
                {
                    EvaluateScoreAppui(rate, selectedRound, word);
                }

                EvaluateScrabble(rate, selectedRound, word, DoScrabble);

                if (!DoBoost)
                {
                    if (DoScrabble)
                    {
                        EvaluateScrabbleSousTop(rate, selectedRound, word, round.SubTops.FirstOrDefault());
                    }
                    else
                    {
                        EvaluateSousTop(rate, selectedRound, word, round.SubTops.FirstOrDefault());
                    }
                }

                EvaluateScoreMot(rate, selectedRound, word);

                if (DoRaccords)
                {
                    EvaluateRaccords(rate, selectedRound, word);
                }

                if (DoRack)
                {
                    EvaluateRack(rate, selectedRound, word, round.SubTops.FirstOrDefault());
                }

            }



            rate.scoreAll = rate.scoreAll / rate.nbSolutions;
            return rate;

        }

        /// <summary>
        /// Evaluate number of solutions possible 
        /// This will be a ratio that will decrease the global rating 
        /// If multiple solutions 
        /// </summary>
        /// <param name="rate"></param>
        /// <param name="round"></param>
        private void EvaluateNumberOfSolutions(RatingRound rate, PlayedRounds round)
        {
            rate.nbSolutions = round.Tops.Count;
        }

        private void EvaluateScoreMot(RatingRound rate, PlayableSolution round, string word)
        {
            //if (round.Bonus == 0)
            {
                if (word.Length > 9)
                {
                    rate.scoremot = 2;
                }

                if (word.Length > 7)
                {
                    rate.scoremot = 1.5f;
                }

                if (word.Length <= 7)
                {
                    rate.scoremot = 1;
                }

                if (round.Tiles.Count(p => p.Parent.Status != 1) < 3)
                {
                    rate.scoremot = -15;
                    rate.scoreAll = -15;
                }
                else
                {
                    rate.scoreAll += rate.scoremot * ScoreMotRatioMul;
                }
            }
        }

        private void EvaluateScoreAppui(RatingRound rate, PlayableSolution round, string word)
        {
            if (round.Tiles.Count(p => p.Parent.Status != 1) < 5)
            {
                return;
            }

            int c = 0;
            for (int x = 0; x < word.Length; x++)
            {
                if (round.Tiles[x].Parent.Status == 1)
                {
                    c++;
                }
            }

            //c = word.Length - c;

            if (c > 1)
            {
                rate.scoreappui = c;
                rate.scoreAll += rate.scoreappui * AppuiRatioMul;
            }
        }

        private void EvaluateCollages(RatingRound rate, PlayableSolution round, string word)
        {
            if (word.Length < 5)
            {
                return;
            }

            int length = word.Length;

            int count = 0; // gc.GameBoard.evalRaccords(tround);
            int words = 0;
            foreach (var l in round.Tiles)
            {
                if (l.Parent.Status == -1)
                {
                    int c = l.Parent.GetPivotLetters(round.Position.Direction);
                    if (c > 0)
                    {
                        count += System.Math.Min(3, c);
                        words++;
                    }
                }
            }

            if (count > 1 && words > 2)
            {
                rate.scorecollage = count;
                rate.scorecollagemots = words;
                rate.scoreAll += ((float)rate.scorecollage * rate.scorecollagemots * CollageRatioMul);
            }
        }
        private void EvaluateRaccords(RatingRound rate, PlayableSolution round, string word)
        {
            if (word.Length > 4)
            {
                int count = CompteRaccord(word);
                rate.scoreraccords = count;
                rate.scoreAll += count * RaccordsRatioMul;
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
            else
            {
                if (totalJoker > 1 || totalPoints < totalLetters + 2)
                {
                    rate.scorerack = -1;
                    rate.scoreAll /= 2;
                }
            }
        }

        private void EvaluateScrabble(RatingRound rate, PlayableSolution round, string word, bool doScrabble)
        {
            if (round.Bonus > 0)
            {
                if (doScrabble)
                {
                    rate.scorescrabble = (float)round.Bonus / ScrabbleRatioDiv;
                    rate.scoreAll += (float)rate.scorescrabble;
                }
                else
                {
                    rate.scorescrabble = -(float)round.Bonus / ScrabbleRatioDiv;
                    rate.scoreAll -= 25;

                }
            }
        }

        private void EvaluateScrabbleSousTop(RatingRound rate, PlayableSolution round, string word, PlayableSolution subTop)
        {
            if (round.Bonus > 0)
            {
                float diff = 100 - subTop.Points / (float)round.Points * 100;
                rate.scoresoustop = diff / SousTopRatioScrabbleDiv;
                rate.scoreAll += rate.scoresoustop;
            }
        }

        private void EvaluateSousTop(RatingRound rate, PlayableSolution round, string word, PlayableSolution subTop)
        {
            float diff = 100 - subTop.Points / (float)round.Points * 100;
            rate.scoresoustop = diff / sousTopRatioDiv;
            rate.scoreAll += rate.scoresoustop;
        }


        private int CompteRaccord(string word)
        {
            GadDagSearch search = new GadDagSearch(currentGame.ControllersSetup.DictionaryContainer.Dico.Root, currentGame.ControllersSetup.DictionaryContainer.TilesUtils);
            var res = search.SearchByPrefix(word, 1);
            res.AddRange(search.SearchBySuffix(word, 1));
            return res.Count;
        }
    }
}
