using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Common.Interfaces.ScrabbleApi;
using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.ScrabbleApi.Components.BoardSolvers;
using Crolow.FastDico.ScrabbleApi.Components.Rounds.Evaluators;
using Crolow.FastDico.ScrabbleApi.Extensions;
using Crolow.FastDico.Utils;

namespace Crolow.FastDico.ScrabbleApi.Components.Rounds
{
    public class BaseRoundValidator : IBaseRoundValidator
    {
        public CurrentGame currentGame;
        public BaseRoundValidator(CurrentGame currentGame)
        {
            this.currentGame = currentGame;
        }

        int maxIterations = 3;
        public virtual void Initialize()
        {
            if (currentGame.ControllersSetup.ReferenceDictionaryContainer != null)
            {
                maxIterations = 15;
            }
        }

        public virtual void InitializeRound()
        {
        }

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

        public virtual List<Tile> InitializeLetters(List<Tile> rack)
        {
            var reject = this.CanRejectBagByDefault(currentGame.GameObjects.GameLetterBag, rack);
            return currentGame.GameObjects.GameLetterBag.DrawLetters(currentGame, rack, reject: reject);
        }
        public virtual PlayedRounds GetRound(List<Tile> letters, SolverFilters filters = null)
        {
            return currentGame.ControllersSetup.BoardSolver.Solve(letters, filters);

        }

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

        public virtual bool CanRejectBagByDefault(LetterBag bag, List<Tile> rack)
        {
            return false;
        }

        public virtual SolverFilters InitializeFilters(bool pickAll = false)
        {
            return new SolverFilters { PickallResults = pickAll };
        }
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

    }
}

