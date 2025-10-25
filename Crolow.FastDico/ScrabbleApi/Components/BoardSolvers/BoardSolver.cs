using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.GadDag;
using Crolow.FastDico.ScrabbleApi.Extensions;
using Crolow.FastDico.Utils;
using Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi;

namespace Crolow.FastDico.ScrabbleApi.Components.BoardSolvers
{
    public class BoardSolver : IBoardSolver
    {
        private CurrentGame currentGame;

        private Board board;

        private IGameConfigModel gameConfig;
        private ILetterNode rootNode;

        public BoardSolver(CurrentGame currentGame)
        {
            this.currentGame = currentGame;
        }

        public void Initialize()
        {
            // Once we get the rack 
            // We create a transposed grid for vertical search
            // We create a grid with all possibilities at each pivot place
            board = currentGame.GameObjects.Board;
            gameConfig = currentGame.GameObjects.GameConfig;
            rootNode = currentGame.ControllersSetup.DictionaryContainer.Dico.Root;

            board.TransposeGrid();
            currentGame.ControllersSetup.PivotBuilder.Build();
        }

        public PlayedRounds Solve(List<Tile> letters, SolverFilters filters = null)
        {
            filters = filters ?? new SolverFilters();

            bool firstMove = currentGame.GameObjects.Round == 0;
            var playedRounds = new PlayedRounds(gameConfig, letters, filters.PickallResults ? 1 : 0);

            // We set the original position to place which is at the board center
            if (firstMove)
            {
                int grid = 0;
                Position p = new Position((board.CurrentBoard[0].SizeH - 1) / 2, (board.CurrentBoard[0].SizeV - 1) / 2, grid);
                playedRounds.CurrentRound = new PlayableSolution();
                playedRounds.CurrentRound.Position = new Position(p);
                board.TransposeGrid();
                SearchNodes(grid, rootNode, 1, p, letters, playedRounds, p, true);
            }
            else
            {
                // Here begins the story... 
                // Related to the DAWG search we are only starting on squares that can
                // be connected 

                // Horizontal Search
                Search(0, letters, playedRounds, filters);
                // Vertical Search
                Search(1, letters, playedRounds, filters);
            }

            return playedRounds;
        }

        private void Search(int grid, List<Tile> letters, PlayedRounds playedRounds, SolverFilters filters)
        {
            for (var i = 1; i < board.CurrentBoard[grid].SizeV - 1; i++)
            {
                if (!filters.Filters[grid].Any() || filters.Filters[grid].Contains(i))
                {
                    var rightToLeft = true;
                    int oldj = -1;
                    for (var j = 1; j < board.CurrentBoard[grid].SizeH - 1; j++)
                    {
                        var currentNode = rootNode;
                        playedRounds.CurrentRound = new PlayableSolution();

                        var sq = board.GetSquare(grid, j, i);

                        if (sq.Status == -1)
                        {
                            if (CheckConnect(grid, j, i))
                            {
                                Position start = new Position(j, i, grid);
                                Position firstPosition = new Position(j, i, grid);
                                // If we are skipping only one square we do not need
                                // to search on the left.
                                rightToLeft = j == 1 || (j != oldj + 1 ? true : false);
                                oldj = j;
                                // If there is a filled squared on the left
                                // We need to prefill the current solution
                                var sqLeft = board.GetSquare(grid, j - 1, i);
                                if (sqLeft.Status == 1)
                                {
                                    rightToLeft = false;
                                    var sql = new List<Square>();
                                    sql.Add(sqLeft);
                                    var pos = j - 2;
                                    while (true)
                                    {
                                        var sqNext = board.GetSquare(grid, pos, i);
                                        if (sqNext.Status == 1)
                                        {
                                            sql.Add(sqNext);
                                            pos--;
                                        }
                                        else
                                        {
                                            sql.Reverse();
                                            playedRounds.CurrentRound = new PlayableSolution();
                                            firstPosition = new Position(pos + 1, firstPosition.Y, grid);
                                            playedRounds.CurrentRound.Position = firstPosition;
                                            int x = firstPosition.X;
                                            int y = firstPosition.Y;

                                            foreach (var item in sql)
                                            {
                                                var parent = board.GetSquare(grid, x++, y);
                                                playedRounds.CurrentRound.AddTile(item.CurrentLetter, parent);
                                            }
                                            break;
                                        }
                                    }

                                    foreach (var letter in sql)
                                    {
                                        currentNode = currentNode.Children.First(p => p.Letter == letter.CurrentLetter.Letter);
                                    }

                                    // Ok we can process that square only if there are children
                                    if (currentNode.Children.Any())
                                    {
                                        SearchNodes(grid, currentNode, 1, start, letters, playedRounds, firstPosition, rightToLeft);
                                    }
                                }
                                else
                                {
                                    SearchNodes(grid, currentNode, 1, start, letters, playedRounds, firstPosition, rightToLeft);
                                }
                                rightToLeft = false;
                            }

                        }

                    }
                    //  break;
                }
                // break;
            }
        }

        private void SearchNodes(int grid, ILetterNode parentNode, int increment, Position p, List<Tile> letters, PlayedRounds rounds, Position firstPosition, bool rightToLeft = true)
        {
            int x = p.X;
            int y = p.Y;

            // We first Get the Square according to the current position
            var square = board.GetSquare(grid, x, y);

            if (square.Status == -1)
            {
                if (rounds.CurrentRound.Tiles.Count(p => p.Parent.Status == 0) >= gameConfig.PlayableLetters)
                {
                    return;
                }
            }


            bool onlyPivot = false;

            if (square.Status == -1 && square.GetPivot(grid) == 0)
            {
                onlyPivot = true;
            }

            // We load the nodes to be checked
            var nodes = new List<ILetterNode>();

            if (square.IsBorder || onlyPivot)
            {
                nodes = parentNode.Children.Where(p => p.Letter == TilesUtils.PivotByte).ToList();
            }
            else
            {
                nodes = parentNode.Children;
            }


            // We set the word/letter multipliers
            Tile tileLetter = new Tile();

            if (square != null && !parentNode.IsPivot)
            {
                if (square.Status == 1)
                {
                    tileLetter = square.CurrentLetter;
                    nodes = nodes.Where(p => p.Letter == tileLetter.Letter).ToList();
                }
            }

            //if (rounds.CurrentRound.GetWord(reorder: false).Equals("U") && x == 4)
            //{
            //    BufferedConsole.WriteLine("U is here");
            //}

            // We go through each node
            foreach (var node in nodes)
            {
                //if (node.Letter == 4 && x == 2)
                //{
                //    BufferedConsole.WriteLine("U is here");
                //}
                // If node is a pivot we need to reset the traversal and invert the direction
                if (!node.IsPivot)
                {
                    var doContinue = true;
                    var didJoker = false;
                    do
                    {
                        var letter = tileLetter;
                        // The current square is empty so we can take a new letter from the rack
                        if (square.Status == -1)
                        {
                            if (!square.GetPivot(grid, node.Letter))
                            {
                                doContinue = false;
                                continue;
                            }

                            var ndxRack = letters.FindIndex(p => /*p.Letter == node.Letter ||*/ !didJoker && p.IsJoker);
                            // No joker found so we can try a letter from the rack
                            if (ndxRack == -1)
                            {
                                didJoker = true;
                                doContinue = false;
                                ndxRack = letters.FindIndex(p => p.Letter == node.Letter);
                            }
                            else
                            {
                                didJoker = true;
                                doContinue = true;
                            }

                            if (ndxRack == -1)
                            {
                                doContinue = false;
                            }

                            if (ndxRack >= 0)
                            {
                                // if the letter is available in the rack or is a joker
                                letter = letters[ndxRack];

                                // Is it possible to place this letter

                                letter.PivotPoints = square.GetPivotPoints(grid);

                                // We remove the letter from the rack
                                //int ndx = letters.FindIndex(p => p.Letter == node.Letter || p.IsJoker);
                                letters.RemoveAt(ndxRack);

                                // if the letter is a joker we asssign assign the current node letter
                                if (letter.IsJoker)
                                {
                                    letter.Letter = node.Letter;
                                }

                                square.Status = 0;
                                rounds.CurrentRound.AddTile(letter, square);
                            }
                            else
                            {
                                continue;
                            }

                        }
                        // We Just add the board tile.
                        else
                        {
                            doContinue = false;
                            rounds.CurrentRound.AddTile(letter, square);
                        }

                        // If the node isEnd we check the round 
                        if (node.IsEnd)
                        {
                            // For a round to be valid the next tile needs to be empty 
                            var nextTile = board.GetSquare(grid, x + increment, y);
                            if (nextTile.Status == -1)
                            {
                                // We update the position of the current word
                                if (firstPosition.ISGreater(p))
                                {
                                    rounds.CurrentRound.Position = new Position(p);
                                }
                                else
                                {
                                    rounds.CurrentRound.Position = new Position(firstPosition);
                                }


                                // We check the and calculate his score
                                rounds.SetRound(rounds.CurrentRound);
                                // We create a new round
                                rounds.CurrentRound = new PlayableSolution(rounds.CurrentRound);
                            }
                        }

                        // if we reach the maximum number of playables we stop 
                        var oldPosition = new Position(x + increment, y, grid);
                        // We continue the search in the nodes 
                        SearchNodes(grid, node, increment, oldPosition, letters, rounds, firstPosition, rightToLeft);
                        rounds.CurrentRound.Position = new Position(oldPosition);
                        // We reset letter on the rack.
                        rounds.CurrentRound.RemoveTile();

                        // If letter comes from rack we put it back
                        if (square.Status == 0)
                        {
                            letters.Add(letter);
                            square.Status = -1;
                        }
                    } while (doContinue);
                }
                else
                {
                    if (rightToLeft == true)
                    {
                        rounds.CurrentRound.SetPivot();

                        Position pp = new Position(firstPosition.X - 1,
                            firstPosition.Y, grid);

                        SearchNodes(grid, node, -1, pp, letters, rounds, firstPosition);
                        rounds.CurrentRound.RemovePivot();
                    }
                }

            }
        }

        public bool ValidateRound(PlayableSolution solution)
        {
            var isValid = true;
            var isPositionValId = false;
            var isFirstRound = currentGame.GameObjects.Round == 0;
            var grid = solution.Position.Direction;
            var tilesFromRack = 0;

            var pos = new Position()
            {
                X = grid == 0 ? solution.Position.X : solution.Position.Y,
                Y = grid == 0 ? solution.Position.Y : solution.Position.X,
                Direction = solution.Position.Direction
            };

            Position middle = new Position((board.CurrentBoard[0].SizeH - 1) / 2, (board.CurrentBoard[0].SizeV - 1) / 2, grid);

            // First we Validate the word
            isValid = new GadDagUtils().CheckWord(currentGame.ControllersSetup.DictionaryContainer.Dico.Root, solution.Tiles);
            if (isValid)
            {
                for (int x = 0; x < solution.Tiles.Count; x++)
                {
                    if (isFirstRound)
                    {
                        if (pos.Equals(middle))
                        {
                            isPositionValId = true;
                            tilesFromRack++;
                            break;
                        }
                        pos.X++;
                    }
                    else
                    {
                        if (CheckConnect(grid, pos.X + x, pos.Y))
                        {
                            isPositionValId = true;
                        }
                        var sq = board.GetSquare(grid, pos.X + x, pos.Y);
                        if (sq.Status != 1)
                        {
                            var l = solution.Tiles[x];
                            tilesFromRack++;
                            if (!sq.GetPivot(l, grid, 0, false))
                            {
                                isValid = false;
                                break;
                            }
                        }
                    }
                }
            }
            return (isValid && isPositionValId && tilesFromRack > 0);
        }

        private bool CheckConnect(int grid, int j, int i)
        {
            if (board.GetSquare(grid, j - 1, i).Status == 1
               || board.GetSquare(grid, j + 1, i).Status == 1
               || board.GetSquare(grid, j, i + 1).Status == 1
               || board.GetSquare(grid, j, i - 1).Status == 1)
            {
                return true;
            }

            return false;
        }

    }
}
