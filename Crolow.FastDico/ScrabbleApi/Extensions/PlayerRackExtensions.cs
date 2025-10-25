using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.Utils;
using Kalow.Apps.Common.Utils;

namespace Crolow.FastDico.ScrabbleApi.Extensions;

public static class PlayerRackExtensions
{
    public static List<Tile> GetTiles(this PlayerRack r)
    {
        return r.Tiles;
    }

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


    // Example: Convert from human-readable string to byte representation
    public static string GetString(this PlayerRack r, ITilesUtils utils)
    {
        return utils.ConvertBytesToWord(r.Tiles.Select(p => p.IsJoker ? TilesUtils.JokerByte : p.Letter).ToList()); // Reuse existing utility
    }
    public static void Clear(this PlayerRack r)
    {
        r.Tiles = new List<Tile>(100);
    }
}
