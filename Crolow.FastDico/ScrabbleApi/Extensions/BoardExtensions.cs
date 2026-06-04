using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.Utils;
using Kalow.Apps.Common.Utils;
namespace Crolow.FastDico.ScrabbleApi.Extensions;

/// <summary>
/// Provides helper methods for reading and mutating Scrabble board grids.
/// </summary>
public static class BoardExtensions
{
    /// <summary>
    /// Gets a square from a board grid when the coordinates are inside the grid bounds.
    /// </summary>
    /// <param name="b">Board that owns the grid.</param>
    /// <param name="grid">Grid index to read.</param>
    /// <param name="col">Column coordinate.</param>
    /// <param name="row">Row coordinate.</param>
    /// <returns>The requested square, or <c>null</c> when the coordinates are outside the grid.</returns>
    public static Square GetSquare(this Board b, int grid, int col, int row)
    {
        if (col < 0 || col >= b.CurrentBoard[grid].Grid.GetLength(1) ||
            row < 0 || row >= b.CurrentBoard[grid].Grid.GetLength(0))
        {
            return null;
        }
        return b.CurrentBoard[grid].Grid[row, col];
    }

    /// <summary>
    /// Clears playable squares on the main grid by removing their current tiles and resetting their status.
    /// </summary>
    /// <param name="b">Board to clear.</param>
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

    /// <summary>
    /// Places a tile on the board and assigns the square status.
    /// </summary>
    /// <param name="b">Board to update.</param>
    /// <param name="grid">Grid index to update.</param>
    /// <param name="col">Column coordinate.</param>
    /// <param name="row">Row coordinate.</param>
    /// <param name="tile">Tile to place.</param>
    /// <param name="status">Status to assign to the square.</param>
    public static void SetTile(this Board b, int grid, int col, int row, Tile tile, int status)
    {
        // WE set definetly the tile on the rack
        b.CurrentBoard[grid].Grid[row, col].CurrentLetter = tile;
        b.CurrentBoard[grid].Grid[row, col].Status = status;
    }

    /// <summary>
    /// Removes the tile from a board square and resets the square status.
    /// </summary>
    /// <param name="b">Board to update.</param>
    /// <param name="grid">Grid index to update.</param>
    /// <param name="col">Column coordinate.</param>
    /// <param name="row">Row coordinate.</param>
    public static void RemoveTile(this Board b, int grid, int col, int row)
    {
        // WE set definetly the tile on the rack
        b.CurrentBoard[grid].Grid[row, col].CurrentLetter = new Tile();
        b.CurrentBoard[grid].Grid[row, col].Status = -1;
    }

    /// <summary>
    /// Applies a playable solution to the board and refreshes the transposed grid.
    /// </summary>
    /// <param name="b">Board to update.</param>
    /// <param name="currentGame">Current game context used for tile and joker handling.</param>
    /// <param name="round">Playable solution to place on the board.</param>
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

    /// <summary>
    /// Updates the secondary grid with a transposed copy of the main grid.
    /// </summary>
    /// <param name="b">Board whose transposed grid is refreshed.</param>
    public static void TransposeGrid(this Board b)
    {
        b.CurrentBoard[1].Grid = ArrayUtils.Transpose(b.CurrentBoard[0].Grid);
    }
}
