using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Common.Interfaces.ScrabbleApi;
using Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi;
using Crolow.FastDico.ScrabbleApi.Components.BoardSolvers;
using Crolow.FastDico.ScrabbleApi.Config;
using Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi;

namespace Crolow.FastDico.Common.Models.ScrabbleApi.Game;

public class CurrentGame
{
    public string FileName { get; set; }

    public GameObjects GameObjects { get; set; }
    public GameControllersSetup ControllersSetup { get; set; }

    public GameHistory History { get; set; } = new GameHistory();
}

public class GameHistory
{
    public IGameDetailModel Game { get; set; }
    public List<IGameUserDetailModel> Users { get; set; } = new List<IGameUserDetailModel>();
}

public class GameObjects
{
    public GameObjects()
    {
    }
    public PlayConfiguration Configuration { get; set; }
    public int Round { get; set; }

    public int MaxRounds { get; set; }
    public GameDetail Rounds { get; set; }
    public GameDetail UserRounds { get; set; }

    public LetterBag GameLetterBag { get; set; }
    public PlayerRack GameRack { get; set; }
    public Board Board { get; set; }
    public GameStatus GameStatus { get; set; }
    public IGameConfigModel GameConfig { get; set; }
    public PlayableSolution SelectedRound { get; set; }
    public ILetterConfigModel LetterConfig { get; set; }
    public PlayedRounds CurrentPlayedRounds { get; set; }
}

public class GameControllersSetup
{
    public IPivotBuilder PivotBuilder;
    public IBoardSolver BoardSolver;
    public IBaseRoundValidator Validator;
    public IScrabbleAI ScrabbleEngine;
    public IScrabbleAIViewer ScrabbleViewEngine;
    public IDictionaryContainer DictionaryContainer;
    public IDictionaryContainer ReferenceDictionaryContainer;
}
public enum GameStatus
{
    None = 0,
    Initialized = 1,
    WaitingToStart = 3,
    WaitingForRack = 4,
    WaitingForNextRound = 5,
    GameEnded = 6,
}
