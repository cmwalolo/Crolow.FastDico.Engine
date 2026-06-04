using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.Utils;
using Kalow.Apps.Common.Utils;
using System.Data;
using System.Text;

namespace Crolow.FastDico.ScrabbleApi.Extensions;

/// <summary>
/// Provides helper methods for drawing, validating, and mutating letters in a Scrabble bag.
/// </summary>
public static class LetterBagExtensions
{
    static Random RandomGen = Random.Shared;

    /// <summary>
    /// Filters a drawn rack for boosted searches by limiting duplicate letters and jokers.
    /// </summary>
    /// <param name="b">Letter bag used as the extension target.</param>
    /// <param name="game">Current game context that defines rack size rules.</param>
    /// <param name="rack">Drawn rack to filter.</param>
    /// <param name="maxJokers">Maximum number of jokers to keep.</param>
    /// <returns>A filtered rack that avoids excessive duplicate letters and jokers.</returns>
    public static List<Tile> Filter(this LetterBag b, CurrentGame game, List<Tile> rack, int maxJokers = 0)
    {
        int inRackLetters = game.GameObjects.GameConfig.InRackLetters;
        var drawnLetters = rack.ToList();
        var groups = rack.Where(p => !p.IsJoker).GroupBy(p => p.Letter);
        foreach (var group in groups)
        {
            if (drawnLetters.Count <= inRackLetters)
            {
                break;
            }
            var values = group.Where(p => !p.IsJoker).ToList();
            while (values.Count > 2)
            {
                var tile = values.First();
                var ndx = drawnLetters.FindIndex(p => p.Letter == tile.Letter && !p.IsJoker);
                drawnLetters.RemoveAt(ndx);
                values.RemoveAt(0);
            }
        }

        var jokers = drawnLetters.Where(p => p.IsJoker).ToArray();
        for (int i = 1; i < jokers.Length; i++)
        {
            if (drawnLetters.Count <= inRackLetters)
            {
                break;
            }

            var ndx = drawnLetters.FindIndex(p => p.IsJoker);
            drawnLetters.RemoveAt(ndx);
        }

        return drawnLetters;
    }

    /// <summary>
    /// Draws letters from the bag while preserving optional rack letters and rack-validity rules.
    /// </summary>
    /// <param name="b">Letter bag to draw from.</param>
    /// <param name="game">Current game context.</param>
    /// <param name="inRackTiles">Tiles already present in the rack.</param>
    /// <param name="totalLetters">Total number of letters to draw; uses the configured rack size when zero.</param>
    /// <param name="reject">Indicates whether invalid initial racks should be rejected before drawing.</param>
    /// <param name="forceInitialTiles">Indicates whether the supplied rack tiles must be kept.</param>
    /// <returns>A valid drawn rack, or <c>null</c> when no valid draw is possible.</returns>
    public static List<Tile> DrawLetters(this LetterBag b, CurrentGame game, List<Tile> inRackTiles, int totalLetters = 0, bool reject = false, bool forceInitialTiles = false)
    {
        // Can we still make a valid rack ?
        if (!b.IsValid(game))
        {
            return null;
        }

        // We make a copy of the bag
        var bagLetters = b.Letters.ToList();
        int inRackLetters = game.GameObjects.GameConfig.InRackLetters;

        int count = totalLetters == 0 ? inRackLetters : totalLetters;

        var drawnLetters = inRackTiles.ToList();

        // Can we by default reject the rack ?
        if (!forceInitialTiles && (reject && !b.IsRackValid(game, drawnLetters)))
        {
            drawnLetters = new List<Tile>();
        }

        // We remove from the bagLetters the existing rack
        if (!forceInitialTiles)
        {
            foreach (var t in drawnLetters)
            {
                var ndx = bagLetters.FindIndex(b => b.Equals(t));
                if (ndx >= 0)
                {
                    bagLetters.RemoveAt(ndx);
                }
            }
        }

        bool ok = true;

        do
        {
            // Rack is not OK we reject and reset the bag
            if (!ok)
            {
                bagLetters = b.Letters.ToList();
                drawnLetters = new List<Tile>();

                if (forceInitialTiles)
                {
                    drawnLetters.AddRange(inRackTiles);
                }
            }


            // In joker mode we take the joker if any
            var jokerMode = game.GameObjects.GameConfig.JokerMode;

            if (jokerMode)
            {
                if (!drawnLetters.Any(p => p.IsJoker))
                {
                    var ndx = bagLetters.FindIndex(p => p.IsJoker);
                    if (ndx > 0)
                    {
                        drawnLetters.Add(bagLetters[ndx]);
                        bagLetters.RemoveAt(ndx);
                    }
                }

                foreach (var t in bagLetters.Where(p => p.IsJoker).ToArray())
                {
                    bagLetters.Remove(t);
                }
            }

            if (drawnLetters.Count == 0 & bagLetters.Count == 0)
            {
                return null;
            }

            while (drawnLetters.Count < count)
            {
                if (bagLetters.Any())
                {
                    int index = RandomGen.Next(bagLetters.Count);
                    drawnLetters.Add(bagLetters[index]);
                    bagLetters.RemoveAt(index);
                    continue;
                }
                break;
            }
            ok = b.IsRackValid(game, drawnLetters);
        } while (!ok);
        return drawnLetters;
    }

    /// <summary>
    /// Removes a matching tile from the bag.
    /// </summary>
    /// <param name="b">Letter bag to update.</param>
    /// <param name="tile">Tile to remove.</param>
    public static void RemoveTile(this LetterBag b, Tile tile)
    {
        var i = b.Letters.FindIndex(t => (!tile.IsJoker && !t.IsJoker && t.Letter == tile.Letter) || t.IsJoker && tile.IsJoker);
        if (i == -1)
        {
            return;
        }
        b.Letters.RemoveAt(i);
    }

    /// <summary>
    /// Removes and returns the first tile matching a letter byte.
    /// </summary>
    /// <param name="b">Letter bag to update.</param>
    /// <param name="letter">Letter byte to remove, or the joker byte to remove a joker.</param>
    /// <returns>The removed tile, or an empty tile when no match is found.</returns>
    public static Tile RemoveTile(this LetterBag b, byte letter)
    {
        var i = b.Letters.FindIndex(t => t.Letter == letter || letter == TilesUtils.JokerByte && t.IsJoker);
        if (i == -1)
        {
            return new Tile();
        }
        var t = b.Letters[i];
        b.Letters.RemoveAt(i);
        return t;
    }

    /// <summary>
    /// Removes each supplied tile from the bag when a matching tile exists.
    /// </summary>
    /// <param name="b">Letter bag to update.</param>
    /// <param name="tiles">Tiles to remove.</param>
    public static void RemoveTiles(this LetterBag b, List<Tile> tiles)
    {
        foreach (var tile in tiles)
        {
            var i = b.Letters.FindIndex(t => (!tile.IsJoker && !t.IsJoker && t.Letter == tile.Letter) || t.IsJoker && tile.IsJoker);
            if (i == -1)
            {
                continue;
            }
            b.Letters.RemoveAt(i);
        }
    }

    /// <summary>
    /// Determines whether a rack satisfies the current vowel, consonant, and joker distribution rules.
    /// </summary>
    /// <param name="b">Letter bag used for fallback distribution checks.</param>
    /// <param name="game">Current game context.</param>
    /// <param name="letters">Rack letters to validate.</param>
    /// <returns><c>true</c> when the rack distribution is valid for the current game state.</returns>
    public static bool IsRackValid(this LetterBag b, CurrentGame game, List<Tile> letters)
    {
        var tileConfig = game.GameObjects.Configuration.TileConfig;

        int vow = letters?.Sum(p => tileConfig.LettersByByte[p.Letter].IsVowel ? 1 : 0) ?? 0;
        int con = letters?.Sum(p => tileConfig.LettersByByte[p.Letter].IsConsonant ? 1 : 0) ?? 0;
        int jok = letters?.Sum(p => tileConfig.LettersByByte[p.Letter].IsJoker ? 1 : 0) ?? 0;

        var result = false;
        if (game.GameObjects.Round > game.GameObjects.GameConfig.CheckDistributionRound)
        {
            result = vow > 0 && con > 0;
        }
        else
        {
            if (game.GameObjects.GameConfig.JokerMode && b.Letters.Count > 7)
            {
                result = vow - jok > 1 && con - jok > 1;
            }
            if (!result)
            {
                result = vow > 1 && con > 1;
                if (!result)
                {
                    int bvow = b.Letters?.Sum(p => tileConfig.LettersByByte[p.Letter].IsVowel ? 1 : 0) ?? 0;
                    int bcon = b.Letters?.Sum(p => tileConfig.LettersByByte[p.Letter].IsConsonant ? 1 : 0) ?? 0;
                    if ((vow == 1 && bvow == 1) || (con == 1 && bcon == 1))
                    {
                        result = true;
                    }

                }
            }
        }


        return result;
    }

    /// <summary>
    /// Determines whether the remaining bag still contains at least one vowel and one consonant.
    /// </summary>
    /// <param name="b">Letter bag to validate.</param>
    /// <param name="game">Current game context.</param>
    /// <returns><c>true</c> when the bag can still produce a valid distribution.</returns>
    public static bool IsValid(this LetterBag b, CurrentGame game)
    {
        var tileConfig = game.GameObjects.Configuration.TileConfig;
        var letters = b.Letters.ToList();

        int vow = letters?.Sum(p => tileConfig.LettersByByte[p.Letter].IsVowel ? 1 : 0) ?? 0;
        int con = letters?.Sum(p => tileConfig.LettersByByte[p.Letter].IsConsonant ? 1 : 0) ?? 0;
        int jok = letters?.Sum(p => tileConfig.LettersByByte[p.Letter].IsJoker ? 1 : 0) ?? 0;

        return vow > 0 && con > 0;
    }

    /// <summary>
    /// Removes the supplied tiles from a copied bag view and returns the original tile list.
    /// </summary>
    /// <param name="b">Letter bag used to check availability.</param>
    /// <param name="tiles">Tiles to force into the draw.</param>
    /// <returns>The supplied tile list.</returns>
    public static List<Tile> ForceDrawLetters(this LetterBag b, List<Tile> tiles)
    {
        var bagLetters = b.Letters.ToList();
        foreach (var tile in tiles)
        {
            var i = bagLetters.FindIndex(t => (!tile.IsJoker && !t.IsJoker && t.Letter == tile.Letter) || t.IsJoker && tile.IsJoker);
            if (i != -1)
            {
                bagLetters.RemoveAt(i);
            }
        }

        return tiles;
    }

    /// <summary>
    /// Builds a forced draw from a word by matching each character against the remaining bag letters.
    /// </summary>
    /// <param name="b">Letter bag to inspect.</param>
    /// <param name="game">Current game context used to resolve letter configuration.</param>
    /// <param name="word">Word whose letters should be drawn.</param>
    /// <returns>Tiles matching the supplied word in order.</returns>
    public static List<Tile> ForceDrawLetters(this LetterBag b, CurrentGame game, string word)
    {
        var letters = b.Letters.ToList();
        var tiles = new List<Tile>();
        foreach (var c in word)
        {
            var tileConfig = game.ControllersSetup.DictionaryContainer.LetterConfig.Letters.FirstOrDefault(p => p.Char[0] == c);
            var i = letters.FirstOrDefault(t => !t.IsJoker && !tileConfig.IsJoker && tileConfig.Letter == t.Letter || t.IsJoker && tileConfig.IsJoker);
            if (!i.IsEmpty)
            {
                var ndx = letters.FindIndex(t => !t.IsJoker && !tileConfig.IsJoker && tileConfig.Letter == t.Letter || t.IsJoker && tileConfig.IsJoker);
                letters.RemoveAt(ndx);
                tiles.Add(i);
            }
        }
        return tiles;
    }

    /// <summary>
    /// Replaces a played joker with an equivalent non-joker tile from the bag when available.
    /// </summary>
    /// <param name="b">Letter bag to inspect.</param>
    /// <param name="tile">Tile that may be replaced.</param>
    /// <param name="drawnLetters">Letters already drawn for the rack.</param>
    /// <returns>The replacement tile when available; otherwise, the original tile.</returns>
    public static Tile ReplaceJoker(this LetterBag b, Tile tile, List<Tile> drawnLetters)
    {
        if (tile.IsJoker)
        {
            var ndx = -1;
            var bagLetters = b.Letters.ToList();
            // We remove from the bagLetters the existing rack
            // We make sure that we do not replace with a letter from the rack
            foreach (var t in drawnLetters)
            {
                ndx = bagLetters.FindIndex(b => b.Equals(t));
                if (ndx >= 0)
                {
                    bagLetters.RemoveAt(ndx);
                }
            }

            // can we Replace the Letter ?
            ndx = bagLetters.FindIndex(p => p.Letter == tile.Letter);
            if (ndx != -1)
            {
                var newTile = bagLetters[ndx];
                return newTile;
            }
        }
        return tile;
    }

    /// <summary>
    /// Writes debug information about the current bag and rack state.
    /// </summary>
    /// <param name="b">Letter bag to describe.</param>
    /// <param name="gg">Current game context.</param>
    /// <param name="rack">Rack to include in the debug output.</param>
    public static void DebugBag(this LetterBag b, CurrentGame gg, PlayerRack rack)
    {
#if DEBUG
        BufferedConsole.WriteLine($"Player rack : {rack.GetString(gg.ControllersSetup.DictionaryContainer.TilesUtils)}");
        var g = b.Letters.GroupBy(p => p.IsJoker ? TilesUtils.JokerByte : p.Letter).OrderBy(p => p.Key);
        var sb = new StringBuilder();
        sb.Append("BAG : ");
        foreach (var l in g)
        {
            char c = l.Key == TilesUtils.JokerByte ? '?' : (char)(l.Key + 65);
            sb.Append($"{c}: {l.Count()} -");
        }
        BufferedConsole.WriteLine(sb.ToString());
        BufferedConsole.WriteLine();
        BufferedConsole.WriteLine($"Letter count : {b.Letters.Count}");
        BufferedConsole.WriteLine("------------------------");

#endif
    }
}
