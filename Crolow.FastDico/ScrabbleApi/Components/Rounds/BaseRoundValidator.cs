using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Common.Interfaces.ScrabbleApi;
using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.ScrabbleApi.Components.BoardSolvers;
using Crolow.FastDico.ScrabbleApi.Components.Rounds.Evaluators;
using Crolow.FastDico.ScrabbleApi.Extensions;
using Crolow.FastDico.Utils;

namespace Crolow.FastDico.ScrabbleApi.Components.Rounds
{
    /// <summary>
    /// Provides the default round validation workflow for drawing letters, solving rounds, filtering results, and finalizing a selected round.
    /// </summary>
    public class BaseRoundValidator : IBaseRoundValidator
    {
        /// <summary>
        /// Current game context controlled by the validator.
        /// </summary>
        public CurrentGame currentGame;

        /// <summary>
        /// Filters applied to rack drawing and solving.
        /// </summary>
        public SolverFilters Filters = new SolverFilters();

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseRoundValidator"/> class.
        /// </summary>
        /// <param name="currentGame">Current game context to validate.</param>
        /// <param name="filters">Optional solver filters used during validation.</param>
        public BaseRoundValidator(CurrentGame currentGame, SolverFilters filters)
        {
            this.currentGame = currentGame;
            if (filters != null) Filters = filters;
        }

        int maxIterations = 3;

        /// <summary>
        /// Initializes game-level validation state before the game starts.
        /// </summary>
        public virtual void Initialize()
        {

            // If the game has letter restrictions 
            // We remove each defined letter from the bag
            if (Filters.LettersInRack.Count > 0 || Filters.MandatoryLettersInRack.Count > 0)
            {
                var l = Filters.LettersInRack.ToList();
                l.AddRange(Filters.MandatoryLettersInRack);
                foreach (var t in l)
                {
                    currentGame.GameObjects.GameLetterBag.RemoveTile(t);
                }
            }
        }

        /// <summary>
        /// Initializes validation state for a new round.
        /// </summary>
        public virtual void InitializeRound()
        {
            if (currentGame.ControllersSetup.ReferenceDictionaryContainer != null)
            {
                maxIterations = 15;
            }
        }

        /// <summary>
        /// Determines whether the game can continue attempting to find valid rounds.
        /// </summary>
        /// <returns><c>true</c> when validation iterations remain.</returns>
        public virtual bool IsValidGame()
        {
#if DEBUG
            if (maxIterations == 0)
            {
                Console.Write("WTF");
            }
#endif 
            return maxIterations > 0;
        }

        /// <summary>
        /// Draws or preserves rack letters for the next solve attempt.
        /// </summary>
        /// <param name="rack">Current rack letters to preserve when possible.</param>
        /// <returns>The letters to solve with, or <c>null</c> when no valid draw is possible.</returns>
        public virtual List<Tile> InitializeLetters(List<Tile> rack)
        {
            var reject = this.CanRejectBagByDefault(currentGame.GameObjects.GameLetterBag, rack);

            if (Filters.ForceStartBoostRound != 0 && currentGame.GameObjects.Round == Filters.ForceStartBoostRound - 1)
            {
                rack = new List<Tile>();
                if (Filters.MandatoryLettersInRack.Count > 0)
                {
                    rack.AddRange(Filters.MandatoryLettersInRack);
                }

                if (Filters.LettersInRack.Count > 0)
                {
                    rack.Add(Filters.LettersInRack[Random.Shared.Next(Filters.LettersInRack.Count)]);
                }

                return currentGame.GameObjects.GameLetterBag.DrawLetters(currentGame, rack, reject: reject, forceInitialTiles: true);
            }

            return currentGame.GameObjects.GameLetterBag.DrawLetters(currentGame, rack, reject: reject, forceInitialTiles: false);
        }

        /// <summary>
        /// Solves a round with the provided letters and filters.
        /// </summary>
        /// <param name="letters">Letters available in the rack.</param>
        /// <param name="filters">Optional solver filters.</param>
        /// <returns>Candidate played rounds produced by the board solver.</returns>
        public virtual PlayedRounds GetRound(List<Tile> letters, SolverFilters filters = null)
        {
            return currentGame.ControllersSetup.BoardSolver.Solve(letters, filters);

        }

        /// <summary>
        /// Validates candidate rounds against the optional reference dictionary.
        /// </summary>
        /// <param name="rounds">Candidate rounds to validate.</param>
        /// <param name="letters">Letters used to produce the rounds.</param>
        /// <param name="solver">Board solver used for the candidate generation.</param>
        /// <returns>The validated rounds, or <c>null</c> when no acceptable top remains.</returns>
        public virtual PlayedRounds ValidateRound(PlayedRounds rounds, List<Tile> letters, IBoardSolver solver)
        {
            if (currentGame.ControllersSetup.ReferenceDictionaryContainer != null)
            {
                var root = currentGame.ControllersSetup.ReferenceDictionaryContainer.Dico.Root;

                foreach (var s in rounds.Tops.ToList())
                {
                    var wordOk = SearchWordRecursive(root, s.Tiles.Select(p => p.Letter).ToList(), 0, false);
                    if (!wordOk) rounds.Tops.Remove(s);
                }

                if (rounds != null && rounds.Tops.Count == 0)
                {
                    maxIterations--;
                    return null;
                }
#if DEBUG
                var Ok = SearchWordRecursive(root, rounds.Tops[0].Tiles.Select(p => p.Letter).ToList(), 0, false);
                var ss = rounds.Tops[0].GetWord(currentGame.ControllersSetup.DictionaryContainer.TilesUtils);
                Console.Write($"{Ok} - {ss}");
#endif
            }

            return rounds;
        }

        /// <summary>
        /// Determines whether an existing rack should be rejected before drawing.
        /// </summary>
        /// <param name="bag">Letter bag used for validation.</param>
        /// <param name="rack">Rack letters to inspect.</param>
        /// <returns><c>true</c> when the rack should be rejected by default.</returns>
        public virtual bool CanRejectBagByDefault(LetterBag bag, List<Tile> rack)
        {
            return false;
        }

        /// <summary>
        /// Initializes and returns solver filters for the next solve operation.
        /// </summary>
        /// <param name="pickAll">Indicates whether all results should be collected.</param>
        /// <returns>The filters to use for solving.</returns>
        public virtual SolverFilters InitializeFilters(bool pickAll = false)
        {
            Filters.PickallResults = pickAll;
            return Filters;
        }

        /// <summary>
        /// Selects, evaluates, and finalizes one playable solution from solved rounds.
        /// </summary>
        /// <param name="playedRounds">Candidate rounds to finalize.</param>
        /// <returns>The selected playable solution, or <c>null</c> when no solution can be finalized.</returns>
        public virtual PlayableSolution FinalizeRound(PlayedRounds playedRounds)
        {
            if (playedRounds.Tops.Count == 0)
            {
                return null;
            }

            /// Filter solutions with and without joker 
            bool isJoker = false;
            bool isWithoutJoker = false;
            foreach (var top in playedRounds.Tops)
            {
                var j = top.Tiles.Count(t => t.IsJoker && t.Parent.Status == -1);
                if (j > 0) isJoker = true;
                if (j == 0) isWithoutJoker = true;
            }

            List<PlayableSolution> tops = new List<PlayableSolution>();
            if (isJoker && isWithoutJoker)
            {
                tops = playedRounds.Tops.Where(t => !t.Tiles.Any(t => t.Parent.Status == -1 && t.IsJoker)).ToList();
            }
            else
            {
                tops = playedRounds.Tops;
            }

            // We take longest from rack 
            tops = tops.GroupBy(t => t.Tiles.Count(p => p.Parent.Status == -1)).OrderByDescending(g => g.Key).First().ToList();

            var rnd = Random.Shared.Next(tops.Count);
            var selectedRound = tops[rnd];

            if (selectedRound.Rack == null)
            {
                selectedRound.Rack = new PlayerRack(playedRounds.PlayerRack);
            }

            if (currentGame.ControllersSetup.ReferenceDictionaryContainer != null)
            {
                var root = currentGame.ControllersSetup.ReferenceDictionaryContainer.Dico.Root;
                var found = SearchWordRecursive(root, selectedRound.Tiles.Select(p => p.Letter).ToList(), 0, false);
                if (!found) return null;
            }

            RoundEvaluator.Evaluate(currentGame, playedRounds, selectedRound);

            selectedRound.FinalizeRound();
            return selectedRound;
        }

        /// <summary>
        /// Recursively checks a word against a dictionary graph while allowing one pivot traversal.
        /// </summary>
        /// <param name="currentNode">Current dictionary node.</param>
        /// <param name="word">Word represented as tile bytes.</param>
        /// <param name="index">Current index in the word.</param>
        /// <param name="pastPivot">Indicates whether the pivot marker has already been traversed.</param>
        /// <returns><c>true</c> when the word is found as a terminal dictionary path.</returns>
        protected bool SearchWordRecursive(ILetterNode currentNode, List<byte> word, int index, bool pastPivot)
        {
            if (index == word.Count)
            {
                return currentNode.IsEnd;
            }

            byte currentByte = word[index];

            // Traverse children to find the matching letter
            foreach (var child in currentNode.Children)
            {
                if (child.Letter == currentByte || !pastPivot && child.Letter == TilesUtils.PivotByte) // Pivot handling
                {
                    if (SearchWordRecursive(child, word, index + 1, pastPivot || child.Letter == TilesUtils.PivotByte))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Gets or sets a board solver placeholder required by the current validator contract.
        /// </summary>
        public BoardSolver BoardSolver
        {
            get => default;
            set
            {
            }
        }

        /// <summary>
        /// Gets or sets a round evaluator placeholder required by the current validator contract.
        /// </summary>
        public RoundEvaluator RoundEvaluator
        {
            get => default;
            set
            {
            }
        }

        /// <summary>
        /// Gets or sets an evaluator placeholder required by the current validator contract.
        /// </summary>
        public Evaluator Evaluator
        {
            get => default;
            set
            {
            }
        }

        /// <summary>
        /// Gets or sets a pivot builder placeholder required by the current validator contract.
        /// </summary>
        public PivotBuilder PivotBuilder
        {
            get => default;
            set
            {
            }
        }
    }
}

