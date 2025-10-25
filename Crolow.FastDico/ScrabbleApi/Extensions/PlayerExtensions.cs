using Crolow.FastDico.Common.Models.ScrabbleApi.Game;

namespace Crolow.FastDico.ScrabbleApi.Extensions;

public static class PlayerExtensions
{
    // Draw a number of letters from the LetterBag
    public static void SetRack(this Player p, Tile[] Letters)
    {
        p.Rack.Tiles.AddRange(Letters);
    }

    // Play a word (remove letters from the rack)
    public static bool PlayWord(this Player p, List<Tile> word)
    {
        return true;
    }
}
