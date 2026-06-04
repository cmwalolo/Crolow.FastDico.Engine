using Crolow.FastDico.Common.Enums;
using Crolow.FastDico.ScrabbleApi.Config;
using Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi;

namespace Crolow.FastDico.Common.Models.ScrabbleApi.Game;

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
