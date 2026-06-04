using Crolow.FastDico.Common.Enums;
using Crolow.FastDico.Common.Interfaces.ScrabbleApi;
using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.ScrabbleApi.Extensions;
using Crolow.FastDico.ScrabbleApi.Utils;
using Kalow.Apps.Common.Utils;
using static Crolow.FastDico.Common.Interfaces.ScrabbleApi.IScrabbleAI;

namespace Crolow.FastDico.ScrabbleApi;

/// <summary>
/// Drives automatic Scrabble game progression, round selection, validation, and end-game notifications.
/// </summary>
public class ScrabbleAI : IScrabbleAI
{
    //public delegate void RoundIsReadyEvent();

    /// <summary>
    /// Occurs when a round has been selected and is waiting for external confirmation.
    /// </summary>
    public event RoundIsReadyEvent? RoundIsReady;

    /// <summary>
    /// Occurs after a round has been applied to the game.
    /// </summary>
    public event RoundSelectedEvent? RoundSelected;

    /// <summary>
    /// Occurs when the game reaches its ended state.
    /// </summary>
    public event GameEndedEvent GameEnded;

    private CurrentGame CurrentGame;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrabbleAI"/> class.
    /// </summary>
    /// <param name="currentGame">Current game state controlled by this engine.</param>
    public ScrabbleAI(CurrentGame currentGame)
    {
        this.CurrentGame = currentGame;
    }

    /// <summary>
    /// Gets or sets the base round validator placeholder required by the current engine contract.
    /// </summary>
    public Components.Rounds.BaseRoundValidator BaseRoundValidator
    {
        get => default;
        set
        {
        }
    }

    /// <summary>
    /// Initializes the game and computes the first round.
    /// </summary>
    /// <returns>A task representing the asynchronous start operation.</returns>
    public async Task StartGame()
    {
        using (StopWatcher stopwatch = new StopWatcher("New game"))
        {
            CurrentGame.ControllersSetup.Validator.Initialize();
        }
        CurrentGame.DebugInfo = BufferedConsole.ReadLog();
        BufferedConsole.ClearBuffer();
        await NextRound();
    }

    /// <summary>
    /// Computes and prepares the next playable round until the game ends or waits for user confirmation.
    /// </summary>
    /// <returns><c>true</c> when a round is prepared or the game completes successfully; otherwise, <c>false</c>.</returns>
    public async Task<bool> NextRound()
    {
        while (CurrentGame.GameObjects.GameStatus != GameStatus.GameEnded)
        {
            using (StopWatcher stopwatch = new StopWatcher("New round"))
            {
                CurrentGame.ControllersSetup.BoardSolver.Initialize();

                PlayedRounds playedRounds = null;
                var letters = new List<Tile>();

                // We create a copy of the rack and the back to
                // Start freshly each iteration
                var originalRack = new PlayerRack(CurrentGame.GameObjects.GameRack);
                var originalBag = new LetterBag(CurrentGame.GameObjects.GameLetterBag);

                CurrentGame.ControllersSetup.Validator.InitializeRound();
                while (true)
                {
                    letters = CurrentGame.ControllersSetup.Validator.InitializeLetters(originalRack.Tiles.ToList());
                    // End Test
                    if (letters == null)

                    {
                        BufferedConsole.WriteLine($"Game is finished : Rack is {CurrentGame.GameObjects.GameLetterBag.IsValid(CurrentGame)}");
                        EndGame();
                        return true;
                    }

                    var filters = CurrentGame.ControllersSetup.Validator.InitializeFilters();
                    playedRounds = CurrentGame.ControllersSetup.Validator.GetRound(letters, filters);
                    CurrentGame.GameObjects.CurrentPlayedRounds = playedRounds;

                    if (playedRounds.Tops.Any())
                    {
                        var round = CurrentGame.ControllersSetup.Validator.ValidateRound(playedRounds, letters, CurrentGame.ControllersSetup.BoardSolver);
                        if (round != null)
                        {
                            playedRounds = round;
                            break;
                        }
                        else if (!CurrentGame.ControllersSetup.Validator.IsValidGame())
                        {
                            EndGame();
                            return false;
                        }
                    }
                    else
                    {
                        BufferedConsole.WriteLine($"Game is finished : No solutions");
                        EndGame();
                        return false;
                    }
                }


                PlayableSolution selectedRound;
                try
                {
                    selectedRound = CurrentGame.ControllersSetup.Validator.FinalizeRound(playedRounds);
                }
                catch (Exception)
                {

                    throw;
                }

                if (selectedRound == null)
                {
                    BufferedConsole.WriteLine($"Game is finished : No finalized round");
                    EndGame();
                    return false;
                }

                CurrentGame.GameObjects.SelectedRound = selectedRound;
                CurrentGame.GameObjects.GameStatus = GameStatus.WaitingForNextRound;
            }


            if (RoundIsReady != null)
            {
                if (CurrentGame.GameObjects.MaxRounds == 0 || CurrentGame.GameObjects.MaxRounds - 1 == CurrentGame.GameObjects.Round)
                {
                    RoundIsReady.Invoke();
                    return true;
                }
                else
                {
                    SetRound();
                }
            }
            else
            {
                SetRound();

                if (CurrentGame.GameObjects.MaxRounds != 0 && CurrentGame.GameObjects.MaxRounds == CurrentGame.GameObjects.Round)
                {
                    EndGame();
                    return false;
                }

            }

            if (CurrentGame.GameObjects.GameStatus == GameStatus.GameEnded)
            {
                return false;
            }
        }
        return false;
    }

    /// <summary>
    /// Applies the selected round to the board, updates rack and bag state, and raises selection notifications.
    /// </summary>
    /// <param name="userSolution">Optional user solution to store alongside the engine-selected round.</param>
    public async void SetRound(PlayableSolution userSolution = null)
    {
        var selectedRound = CurrentGame.GameObjects.SelectedRound;

        CurrentGame.GameObjects.Board.SetRound(CurrentGame, selectedRound);

        // We reorder the 
        selectedRound.Rack.Tiles = selectedRound.Rack.Tiles.OrderBy(p => p.IsJoker).ThenBy(p => p.Letter).ToList();

        // We need to create a new rack and update the bag
        CurrentGame.GameObjects.GameRack = new PlayerRack(selectedRound.Rack);
        foreach (Tile t in selectedRound.Tiles.Where(p => p.Source == -1))
        {
            if (t.IsJokerReplaced)
            {
                var jt = CurrentGame.GameObjects.GameRack.GetTiles().Find(p => p.IsJoker);
                if (!jt.IsEmpty)
                {
                    CurrentGame.GameObjects.GameRack.RemoveTile(jt);
                }
            }
            else
            {
                CurrentGame.GameObjects.GameRack.RemoveTile(t);

            }
            CurrentGame.GameObjects.GameLetterBag.RemoveTile(t);

        }

        if (CurrentGame.GameObjects.Round == CurrentGame.GameObjects.Rounds.Rounds.Count)
        {
            var r = CurrentGame.GameObjects.Rounds;
            r.Rounds.Add(selectedRound);
            r.TotalPoints += selectedRound.Points;
            r.PlayTime += selectedRound.PlayedTime;

            if (userSolution == null)
            {
                userSolution = new PlayableSolution();
            }

            if (CurrentGame.GameObjects.UserRounds != null)
            {
                r = CurrentGame.GameObjects.UserRounds;
                r.Rounds.Add(userSolution);
                r.TotalPoints += userSolution.Points;
                r.PlayTime += userSolution.PlayedTime;
            }
        }

        CurrentGame.GameObjects.Round++;

#if DEBUG
        CurrentGame.GameObjects.GameLetterBag.DebugBag(CurrentGame, selectedRound.Rack);
#endif

        if (RoundSelected != null)
        {
            RoundSelected.Invoke(selectedRound, selectedRound.Rack);
        }
    }

    /// <summary>
    /// Marks the current game as ended and raises the game-ended event.
    /// </summary>
    public void EndGame()
    {
        CurrentGame.GameObjects.GameStatus = GameStatus.GameEnded;
        GameEnded?.Invoke();
    }

    /// <summary>
    /// Validates a proposed solution and returns its score.
    /// </summary>
    /// <param name="solution">Solution to validate.</param>
    /// <param name="forceTop">Indicates whether the score should be taken from the current top solution when matched.</param>
    /// <returns>The validated score, or zero when invalid.</returns>
    public async Task<int> ValidateRound(PlayableSolution solution, bool forceTop)
    {
        var isValid = CurrentGame.ControllersSetup.BoardSolver.ValidateRound(solution);
        if (!isValid)
        {
            solution.Points = 0;
        }

        if (forceTop)
        {
            var src = solution.GetWord(CurrentGame.ControllersSetup.DictionaryContainer.TilesUtils);

            var tgt = CurrentGame.GameObjects.CurrentPlayedRounds.Tops.FirstOrDefault(p => p.GetWord(CurrentGame.ControllersSetup.DictionaryContainer.TilesUtils).Equals(src));
            if (tgt != null)
            {
                solution.Points = tgt.Points;
            }
        }

        return solution.Points;
    }

    /// <summary>
    /// Validates and applies a proposed solution as the finalized round.
    /// </summary>
    /// <param name="solution">Solution to validate and apply.</param>
    /// <param name="forceTop">Indicates whether a matching top solution should replace the submitted solution.</param>
    /// <returns><c>true</c> when the proposed solution is valid; otherwise, <c>false</c>.</returns>
    public async Task<bool> FinalizeRound(PlayableSolution solution, bool forceTop)
    {
        var isValid = CurrentGame.ControllersSetup.BoardSolver.ValidateRound(solution);
        if (!isValid)
        {
            solution.Points = 0;
        }

        if (forceTop)
        {
            var src = solution.GetWord(CurrentGame.ControllersSetup.DictionaryContainer.TilesUtils);

            var tgt = CurrentGame.GameObjects.CurrentPlayedRounds.Tops.FirstOrDefault(p => p.GetWord(CurrentGame.ControllersSetup.DictionaryContainer.TilesUtils).Equals(src));
            if (tgt != null)
            {
                solution = tgt;
            }
        }

        SetRound(solution);
        return isValid;
    }
}
