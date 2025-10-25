#undef DEBUGPIVOT  

using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.GadDag;
using Crolow.FastDico.ScrabbleApi.Config;
using Crolow.FastDico.ScrabbleApi.Extensions;
using Crolow.FastDico.Search;
using Crolow.FastDico.Utils;
using System.Security.Authentication.ExtendedProtection;
using System.Text;


namespace Crolow.FastDico.ScrabbleApi.Components.BoardSolvers
{
    public class PivotBuilder : IPivotBuilder
    {
        private Dictionary<string, uint> PivotCache = new Dictionary<string, uint>();
        private Board board;
        private IDawgSearch searcher;
        private ILetterNode letterNode;
        private PlayConfiguration playConfiguration;
        private CurrentGame currentGame;

        public PivotBuilder(CurrentGame currentGame)
        {
            this.currentGame = currentGame;
            this.board = currentGame.GameObjects.Board;
            this.searcher = currentGame.ControllersSetup.DictionaryContainer.Searcher;
            letterNode = currentGame.ControllersSetup.DictionaryContainer.Dico.Root;
            this.playConfiguration = currentGame.GameObjects.Configuration;
            PivotCache.Clear();
        }

        public uint[] GetMask(int x, int y, int direction)
        {
            return board.GetSquare(direction, x, y).Pivots;
        }

        public void Build()
        {
            Build(0, 1);
            Build(1, 0);
        }

        public void Build(int grid, int targetGrid)
        {
            for (int y = 1; y < board.CurrentBoard[0].SizeV - 1; y++)
            {
                var squares = new Square[board.CurrentBoard[0].SizeH];

                for (int x = 0; x < board.CurrentBoard[0].SizeH; x++)
                {
                    squares[x] = board.GetSquare(grid, x, y);
                    squares[x].ResetPivot(targetGrid, 0);
                }
                MaskLeftToRightHorizontal(grid, targetGrid, squares, y);
            }

        }

        private struct Run(bool toRight, int start, int end)
        {
            public bool toRight = toRight;
            public int start = start, end = end;
        }

        private void MaskLeftToRightHorizontal(int grid, int targetGrid, Square[] squares, int y)
        {
            int startX = 1;
            int endX = 0;

            var runs = new List<Run>();

            if (squares.Count(p => p.Status == 1) == 0)
            {
                return;
            }

            // We first calculate runs of adjacent letters
            do
            {
                while (!squares[startX].IsBorder && squares[startX].Status == -1)
                {
                    startX++;
                }

                endX = startX;
                var extend = false;
                if (!squares[startX].IsBorder)
                {

                    // We are looking for adjactent letters for the pivots to the left
                    while (!squares[endX].IsBorder && squares[endX].Status == 1)
                    {
                        endX++;
                    }

                    var run = new Run(true, startX, endX - 1);
                    runs.Add(run);
                }
                startX = endX + 1;
            } while (startX < squares.Length && !squares[endX].IsBorder);

            for (int x = 0; x < runs.Count; x++)
            {
                Run runtoDo = new Run();
                var run = runs[x];

                // Doing left Pivot
                if (run.start > 1)
                {
                    // if not first run there is at least two squares between runs
                    if (x == 0 || runs[x].start - 2 > runs[x - 1].end)
                    {
                        runtoDo = new Run { start = run.start - 1, end = run.end };
                    }

                    if (runtoDo.start > 0)
                    {
                        Solve(grid, targetGrid, runtoDo, squares);
                    }
                }

                // We proceed right 
                runtoDo = new Run();
                if (run.end < squares.Length - 1)
                {
                    if (x == runs.Count - 1 || runs[x].end < runs[x + 1].start - 2)
                    {
                        runtoDo = new Run { start = run.start, end = run.end + 1 };
                    }
                    else
                    {
                        runtoDo = new Run { start = run.start, end = runs[x + 1].end };
                    }
                    if (runtoDo.start > 0)
                    {
                        Solve(grid, targetGrid, runtoDo, squares);
                    }
                }
            }
            return;

        }

        private void Solve(int grid, int targetGrid, Run run, Square[] squares)
        {
            var start = run.start;
            var end = run.end;

            var bytes = new byte[end - start + 1];
            uint key = 0;
            var points = 0;
            var pivot = uint.MaxValue;
            var pivotPosition = 0;
            var pivotLetters = 0;
            Square pivotSquare = null;

            for (int i = start; i <= end; i++)
            {
                var sq = squares[i];
                if (sq.Status == -1)
                {
                    bytes[i - start] = TilesUtils.JokerByte;
                    pivotSquare = squares[i];
                    pivotPosition = i - start;
                }
                else
                {
                    bytes[i - start] = sq.CurrentLetter.Letter;
                    points += sq.CurrentLetter.Points;
                    pivotLetters++;
                }
            }

            pivotSquare.ResetPivot(targetGrid, 0, 0);
            if (pivotLetters > 0)
            {
                pivotSquare.SetPivotLetters(pivotLetters, targetGrid);
                pivotSquare.SetPivotPoints(targetGrid, points);

            }

            var pattern = currentGame.ControllersSetup.DictionaryContainer.TilesUtils.ConvertBytesToWord(bytes.ToList());
            if (PivotCache.ContainsKey(pattern))
            {
                pivot = PivotCache[pattern];
                pivotSquare.SetPivot(pivot, targetGrid);
            }
            else
            {
                var results = SearchByPattern(bytes);

                if (results.Any())
                {

                    foreach (var result in results)
                    {
                        pivotSquare.SetPivot(result[pivotPosition], targetGrid);
                    }
                }

                var mask = pivotSquare.GetPivot(targetGrid);
                PivotCache.Add(pattern, mask);
            }

#if DEBUGPIVOT
            var pattern = TilesUtils.ConvertBytesToWord(bytes.ToList());
            var mask = pivotSquare.Pivots[targetGrid];
            var pts = pivotSquare.PivotPoints[targetGrid];
            BufferedConsole.WriteLine($"raccord : {pattern} {mask} {pts}");
#endif

        }

        public List<byte[]> SearchByPattern(byte[] bytePattern)
        {
            // Convert the pattern into bytes
            List<byte[]> results = new List<byte[]>();
            SearchByPatternRecursive(letterNode, bytePattern, 0, new byte[bytePattern.Length], results);
            return results;
        }

        private void SearchByPatternRecursive(ILetterNode currentNode, byte[] bytePattern, int patternIndex, byte[] currentWord, List<byte[]> results)
        {
            // Base case: Reached the end of the pattern
            if (patternIndex == bytePattern.Length)
            {
                if (currentNode.IsEnd)
                {
                    results.Add(currentWord.ToArray());
                }
                return;
            }

            byte currentByte = bytePattern[patternIndex];

            if (currentByte == TilesUtils.JokerByte) // '?' wildcard
            {
                // Match exactly one character
                foreach (var child in currentNode.Children)
                {
                    if (child.Letter != TilesUtils.PivotByte)
                    {
                        currentWord[patternIndex] = child.Letter;
                        SearchByPatternRecursive(child, bytePattern, patternIndex + 1, currentWord, results);
                        currentWord[patternIndex] = TilesUtils.JokerByte;
                    }
                }
            }
            else
            {
                // Match the exact character
                var nextNode = currentNode.Children.Where(p => p.Letter != TilesUtils.PivotByte && p.Letter == currentByte);
                if (nextNode.Any() && currentByte != TilesUtils.PivotByte)
                {
                    currentWord[patternIndex] = currentByte;
                    SearchByPatternRecursive(nextNode.First(), bytePattern, patternIndex + 1, currentWord, results);
                    currentWord[patternIndex] = TilesUtils.JokerByte; // Backtrack
                }
            }
        }
    }
}
