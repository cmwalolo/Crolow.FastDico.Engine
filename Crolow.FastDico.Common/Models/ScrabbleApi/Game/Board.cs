using Crolow.FastDico.ScrabbleApi.Config;
using Crolow.FastDico.Utils;

namespace Crolow.FastDico.Common.Models.ScrabbleApi.Game;

public class Board
{
    // Current State of the board
    public GridConfigurationContainer[] CurrentBoard { get; set; } = new GridConfigurationContainer[2];

    public Board(GameObjects currentGame)
    {
        CurrentBoard[0] = new GridConfigurationContainer(currentGame.Configuration.GridConfig);
        CurrentBoard[1] = new GridConfigurationContainer();
        CurrentBoard[1].SizeV = CurrentBoard[0].SizeH;
        CurrentBoard[1].SizeH = CurrentBoard[0].SizeV;
        CurrentBoard[1].Grid = ArrayUtils.Transpose(CurrentBoard[0].Grid);
    }
}
