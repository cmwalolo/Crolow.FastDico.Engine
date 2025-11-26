using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.ScrabbleApi.Components.BoardSolvers;
using Crolow.FastDico.ScrabbleApi.Components.Rounds.Evaluators;
using Crolow.FastDico.ScrabbleApi.Extensions;
using Kalow.Apps.Common.Utils;
using static Crolow.FastDico.ScrabbleApi.Components.Rounds.Evaluators.Evaluator;

namespace Crolow.FastDico.ScrabbleApi.Components.Rounds
{

    public class XRoundValidator : BaseRoundValidator
    {

        int maxLettersInRack = 100;
        int currentIteration = 0;

        int[] maxIteration = new int[] { 50, 30, 15 };
        int[] breakPoints = new int[] { 30, 25, 20 };

        private int boostNumberOfSolutions = 30000;
        private int boostMatchItems = 10000;

        private Evaluator evaluator;
        private RatingRound bestRate;
        private PlayedRounds bestRounds;

        public XRoundValidator(CurrentGame currentGame, SolverFilters filters) : base(currentGame, filters)
        {
            evaluator = new Evaluator(currentGame);
        }


        public override void Initialize()
        {
            base.Initialize();
        }

        public override void InitializeRound()
        {
            currentIteration = 0;
            maxIteration = new int[] { 2 };
            breakPoints = new int[] { 30, 15 };

            evaluator.Initialize();
            bestRate = null;
            bestRounds = null;
        }

        public override bool IsValidGame()
        {
            return evaluator.IsBoosted() || maxIteration[maxIteration.Count() - 1] > 0;
        }

        public override List<Tile> InitializeLetters(List<Tile> rack)
        {
            var reject = this.CanRejectBagByDefault(currentGame.GameObjects.GameLetterBag, rack);

            if (Filters.ForceStartBoostRound != 0)
            {
                if (currentGame.GameObjects.Round < Filters.ForceStartBoostRound - 1)
                {
                    return base.InitializeLetters(rack);
                }
            }

            // This one is only used for backup if no
            // boosted solution found or if the BoostSequence didn't trigger
            if (!evaluator.IsBoosted())
            {
                return currentGame.GameObjects.GameLetterBag.DrawLetters(currentGame, rack, reject: reject);
            }
            else
            {
                var min = Math.Max(Filters.MinimalRack, currentGame.GameObjects.GameConfig.InRackLetters);
                var max = currentGame.GameObjects.GameLetterBag.RemainingLetters;
                if (max < min) max = min;

                max = (int)Random.Shared.Next(min, (int)max);

#if DEBUG
                BufferedConsole.WriteLine($"Boosting with {max} letters");
#endif
                var letters = currentGame.GameObjects.GameLetterBag.DrawLetters(currentGame, rack, max, reject);
                if (letters != null)
                {
                    // We filter letters to not have more then two same tile in the rack, and max 1 joker
                    // This is to increase the speed of the search dramatically
                    letters = currentGame.GameObjects.GameLetterBag.Filter(currentGame, letters, 1);
                }

                return letters;
            }
        }

        public override SolverFilters InitializeFilters(bool pickAll = false)
        {
            Filters.PickallResults = evaluator.IsBoosted();
            return Filters;
        }

        public override bool CanRejectBagByDefault(LetterBag bag, List<Tile> rack)
        {
            return !bag.IsRackValid(currentGame, rack);
        }

        public override PlayedRounds ValidateRound(PlayedRounds rounds, List<Tile> letters, IBoardSolver solver)
        {
            if (Filters.ForceStartBoostRound != 0 && currentGame.GameObjects.Round < Filters.ForceStartBoostRound - 1)
            {
                currentIteration = maxIteration.Count();
                return base.ValidateRound(rounds, letters, solver);
            }

            if (solver is null)
            {
                throw new ArgumentNullException(nameof(solver));
            }

            if (currentGame.GameObjects.Round == 0)
            {
                return rounds;
            }

            if (evaluator.IsBoosted())
            {
                rounds = ValidateBoosted(rounds, solver);
                if (rounds == null)
                {
                    maxIteration[currentIteration]--;

                    if (maxIteration[currentIteration] <= 0)
                    {
                        evaluator.BoostedOff();
                        maxIteration = new int[] { 30, 15 };
                    }
                }

                return rounds;
            }
            else
            {
                if (currentGame.ControllersSetup.ReferenceDictionaryContainer != null)
                {
                    foreach (var s in rounds.Tops.ToList())
                    {
                        var root = currentGame.ControllersSetup.ReferenceDictionaryContainer.Dico.Root;
                        var wordOk = SearchWordRecursive(root, s.Tiles.Select(p => p.Letter).ToList(), 0, false);
                        if (!wordOk) rounds.Tops.Remove(s);
                    }
                }

                if (rounds.Tops.Count > 0)
                {

                    var rate = evaluator.Evaluate(rounds);

                    if (rate.scoreAll > breakPoints[currentIteration])
                    {
                        DebugRatingRound(rate);
                        return rounds;
                    }
                    else
                    {
                        maxIteration[currentIteration]--;

                        if (maxIteration[currentIteration] <= 0)
                        {
                            currentIteration++;
                            if (currentIteration == maxIteration.Length)
                            {
                                DebugRatingRound(bestRate);
                                return bestRounds;
                            }
                            else
                            {
                                if (bestRate != null && (bestRate.scoreAll > breakPoints[currentIteration]))
                                {
                                    DebugRatingRound(bestRate);
                                    return bestRounds;
                                }
                            }
                        }

                        if (bestRate == null || rate.scoreAll > bestRate.scoreAll)
                        {
                            bestRate = rate;
                            bestRounds = rounds;
                        }

                        return null;
                    }
                }
                return null;
            }
        }

        private PlayedRounds ValidateBoosted(PlayedRounds playedRounds, IBoardSolver solver)
        {
#if DEBUG
            BufferedConsole.WriteLine("--- BOOSTED --- ");
#endif
            if (playedRounds.Tops.Count == 0)
            {
                return null;
            }


            var solutions = new List<PlayableSolution>();

            var allRounds = playedRounds.AllRounds.GetBest();
            if (currentGame.GameObjects.GameConfig.JokerMode)
            {
                solutions = allRounds
                    .Distinct()
                    .Where(p => p.Tiles.Count(t => t.Parent.Status != 1) > 4 && p.Tiles.Any(p => p.IsJoker)).ToList();
            }

            if (!solutions.Any())
            {
                solutions = allRounds.Distinct().Where(p => p.Tiles.Count(t => t.Parent.Status != 1) > 4).OrderByDescending(p => p.Points).ToList();
            }

            int maxIterations = 10;
            if (!evaluator.DoScrabble)
            {
                maxIterations = 30;
                solutions = solutions.Where(p => p.Bonus == 0).ToList();
            }

            solutions = solutions.OrderByDescending(p => p.Points)/*.Take(100000)*/.ToList();

            var selection = new Dictionary<RatingRound, PlayableSolution>();
            var counter = 0;
            foreach (var solution in solutions)
            {
                var rate = evaluator.Evaluate(playedRounds, solution);
                if (rate.scoreAll > 25 && (rate.scoreappui > 2 || rate.scorecollage > 5))
                {
                    selection.Add(rate, solution);
                }

                counter++;

                if (selection.Count > boostMatchItems && counter < boostNumberOfSolutions)
                {
                    break;
                }
            }

#if DEBUG
            BufferedConsole.WriteLine($"BOOSTING {allRounds.Count} - {solutions.Count} - matches : {selection.Count} ");
#endif 

            if (!selection.Any())
            {
                return null;
            }

            var count = 15 - currentGame.GameObjects.Configuration.SelectedConfig.PlayableLetters;
            var take = count * 80;


            // We create a group with a random value to take random solution in group of 10 scoreAll 
            // This is to make sure we do not always opt for TOP of TOP solution  

            var keys = selection.OrderByDescending(p => System.Math.Floor((double)p.Key.scoreAll / 10f) + Random.Shared.NextDouble()/* (p.Key.scoreappui * 2) + p.Key.scorecollage*/).Take(take);

            // For each solution we need to check that the selected solution
            // with a rack is the top score. We only do one trial. 
            // If solution is not top score we pass to next . 
            // If no solution is ok we return a null and 
            // process will fallback to unboosted validation

            bool found = false;
            foreach (var value in keys)
            {
                for (int x = 0; x < maxIterations; x++)
                {
                    var selectedSolution = value.Value;

                    if (currentGame.ControllersSetup.ReferenceDictionaryContainer != null)
                    {
                        var root = currentGame.ControllersSetup.ReferenceDictionaryContainer.Dico.Root;
                        var wordOk = SearchWordRecursive(root, selectedSolution.Tiles.Select(p => p.Letter).ToList(), 0, false);
                        if (!wordOk) continue;
                    }

                    var rack = new PlayerRack(value.Value.Tiles.Where(p => p.Parent.Status == -1).ToList());

                    // No need to try out different racks as it is full
                    if (rack.Tiles.Count == currentGame.GameObjects.GameConfig.PlayableLetters)
                    {
                        x = maxIterations;
                    }

                    var letters = currentGame.GameObjects.GameLetterBag.ForceDrawLetters(rack.Tiles.ToList());
                    letters = currentGame.GameObjects.GameLetterBag.DrawLetters(currentGame, rack.Tiles.ToList());
                    var round = solver.Solve(letters);
                    selectedSolution.Rack = new PlayerRack(letters);
                    var checkSolution = round.Tops.FirstOrDefault();

                    if (selectedSolution.Points == checkSolution.Points)
                    {
                        found = true;
                        break;
                    }
                }

                if (found)
                {
#if DEBUG
                    BufferedConsole.WriteLine($"BOOSTED");
                    DebugRatingRound(value.Key);
#endif

                    playedRounds.AllRounds.Clear();
                    playedRounds.SubTops.Clear();
                    playedRounds.Tops.Clear();
                    playedRounds.Tops.Add(value.Value);
                    return playedRounds;
                }
            }
            return null;

        }

        public override PlayableSolution FinalizeRound(PlayedRounds playedRounds)
        {
            return base.FinalizeRound(playedRounds);
        }

        public void DebugRatingRound(RatingRound round)
        {
#if DEBUG
            if (round != null)
            {
                BufferedConsole.WriteLine("Score rack : " + round.scorerack);
                BufferedConsole.WriteLine("Score collage  : " + round.scorecollage);
                BufferedConsole.WriteLine("Score collagemots : " + round.scorecollagemots);
                BufferedConsole.WriteLine("Score mot : " + round.scoremot);
                BufferedConsole.WriteLine("Score raccords : " + round.scoreraccords);
                BufferedConsole.WriteLine("Score scrabble : " + round.scorescrabble);
                BufferedConsole.WriteLine("Score soustop : " + round.scoresoustop);
                BufferedConsole.WriteLine("Score appui : " + round.scoreappui);
                BufferedConsole.WriteLine("Overall : " + round.scoreAll);
                BufferedConsole.WriteLine("---------------------------------");
            }
#endif
        }
    }
}
