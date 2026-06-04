using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.Utils;
using Kalow.Apps.Common.Utils;

namespace Crolow.FastDico.ScrabbleApi.Extensions;

/// <summary>
/// Provides helper methods for reading and mutating a player's rack.
/// </summary>
public static class PlayerRackExtensions
{
    /// <summary>
    /// Gets the tiles currently held by the rack.
    /// </summary>
    /// <param name="r">Rack to read.</param>
    /// <returns>The rack tile list.</returns>
    public static List<Tile> GetTiles(this PlayerRack r)
    {
        return r.Tiles;
    }

    /// <summary>
    /// Removes the first matching tile from the rack.
    /// </summary>
    /// <param name="r">Rack to update.</param>
    /// <param name="tile">Tile to remove.</param>
    public static void RemoveTile(this PlayerRack r, Tile tile)
    {
        var i = r.Tiles.FindIndex(t => (!t.IsJoker && !tile.IsJoker && t.Letter == tile.Letter) || t.IsJoker && tile.IsJoker);
        if (i == -1)
        {
            BufferedConsole.WriteLine($"Rack missing tile : {tile.Letter}");
            return;
        }
        r.Tiles.RemoveAt(i);
    }


    /// <summary>
    /// Converts the rack tiles to a display string.
    /// </summary>
    /// <param name="r">Rack to convert.</param>
    /// <param name="utils">Tile utility used to convert bytes to a word.</param>
    /// <returns>The rack as a display word, rendering jokers with the joker byte.</returns>
    public static string GetString(this PlayerRack r, ITilesUtils utils)
    {
        return utils.ConvertBytesToWord(r.Tiles.Select(p => p.IsJoker ? TilesUtils.JokerByte : p.Letter).ToList()); // Reuse existing utility
    }

    /// <summary>
    /// Clears all tiles from the rack.
    /// </summary>
    /// <param name="r">Rack to clear.</param>
    public static void Clear(this PlayerRack r)
    {
        r.Tiles = new List<Tile>(100);
    }
}
