using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.Utils;
using Kalow.Apps.Common.Utils;
namespace Crolow.FastDico.ScrabbleApi.Extensions;

public static class BoardExtensions
{
    public static Square GetSquare(this Board b, int grid, int col, int row)
    {
        if (col < 0 || col >= b.CurrentBoard[grid].Grid.GetLength(1) ||
            row < 0 || row >= b.CurrentBoard[grid].Grid.GetLength(0))
        {
            return null;
        }
        return b.CurrentBoard[grid].Grid[row, col];
    }

    public static void ClearGrid(this Board b)
    {
        int rows = b.CurrentBoard[0].Grid.GetLength(0);
        int cols = b.CurrentBoard[0].Grid.GetLength(1);

        for (int i = 1; i < rows - 1; i++)
        {
            for (int j = 0; j < cols - 1; j++)
            {
                b.CurrentBoard[0].Grid[i, j].Status = -1;
                b.CurrentBoard[0].Grid[i, j].CurrentLetter = new Tile();
            }
        }
    }

    public static void SetTile(this Board b, int grid, int col, int row, Tile tile, int status)
    {
        // WE set definetly the tile on the rack
        b.CurrentBoard[grid].Grid[row, col].CurrentLetter = tile;
        b.CurrentBoard[grid].Grid[row, col].Status = status;
    }

    public static void RemoveTile(this Board b, int grid, int col, int row)
    {
        // WE set definetly the tile on the rack
        b.CurrentBoard[grid].Grid[row, col].CurrentLetter = new Tile();
        b.CurrentBoard[grid].Grid[row, col].Status = -1;
    }

    public static void SetRound(this Board b, CurrentGame currentGame, PlayableSolution round)
    {
        int incH = 0;
        int incV = 0;
        int x = round.Position.X;
        int y = round.Position.Y;

        round.DebugRound(currentGame.ControllersSetup.DictionaryContainer.TilesUtils, "Word");

        if (round.Position.Direction == 0)
        {
            incH = 1;
        }
        else
        {
            incV = 1;
        }

        var isJoker = currentGame.GameObjects.Configuration.SelectedConfig.JokerMode;
        var tiles = round.Tiles.ToArray();
        round.Tiles = new List<Tile>();
        foreach (var tile in tiles)
        {
            var newTile = new Tile(tile, tile.Parent);
            if (isJoker && newTile.IsJoker && tile.Source == -1)
            {
                newTile = currentGame.GameObjects.GameLetterBag.ReplaceJoker(newTile, round.Rack.Tiles);
                if (!newTile.IsJoker)
                {
                    newTile.IsJokerReplaced = true;
                    newTile.Source = -1;
                }
            }

            round.Tiles.Add(newTile);
            b.SetTile(0, x, y, newTile, 1);
            x += incH;
            y += incV;
        }

        BufferedConsole.WriteLine("-------------------------------------------");

        b.TransposeGrid();
    }

    public static void TransposeGrid(this Board b)
    {
        b.CurrentBoard[1].Grid = ArrayUtils.Transpose(b.CurrentBoard[0].Grid);
    }
}
