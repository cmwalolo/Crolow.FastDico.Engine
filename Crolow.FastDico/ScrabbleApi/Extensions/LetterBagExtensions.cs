using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.Utils;
using Kalow.Apps.Common.Utils;
using System.Data;
using System.Text;

namespace Crolow.FastDico.ScrabbleApi.Extensions;

public static class LetterBagExtensions
{
    static Random RandomGen = Random.Shared;

    /// <summary>
    /// For the boosted version we Filter the bag to not have more then 2 letters the same
    /// to prevent numerous identical solutions to be solved.
    /// </summary>
    /// <param name="b"></param>
    /// <param name="rack"></param>
    /// <param name="maxJokers"></param>
    /// <returns></returns>
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

    public static List<Tile> DrawLetters(this LetterBag b, CurrentGame game, List<Tile> inRackTiles, int totalLetters = 0, bool reject = false)
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
        if (reject && !b.IsRackValid(game, drawnLetters))
        {
            drawnLetters = new List<Tile>();
        }

        // We remove from the bagLetters the existing rack
        foreach (var t in drawnLetters)
        {
            var ndx = bagLetters.FindIndex(b => b.Equals(t));
            if (ndx >= 0)
            {
                bagLetters.RemoveAt(ndx);
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

    public static void RemoveTile(this LetterBag b, Tile tile)
    {
        var i = b.Letters.FindIndex(t => (!tile.IsJoker && !t.IsJoker && t.Letter == tile.Letter) || t.IsJoker && tile.IsJoker);
        if (i == -1)
        {
            return;
        }
        b.Letters.RemoveAt(i);
    }

    // Get the number of letters remaining in the bag

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

    public static bool IsValid(this LetterBag b, CurrentGame game)
    {
        var tileConfig = game.GameObjects.Configuration.TileConfig;
        var letters = b.Letters.ToList();

        int vow = letters?.Sum(p => tileConfig.LettersByByte[p.Letter].IsVowel ? 1 : 0) ?? 0;
        int con = letters?.Sum(p => tileConfig.LettersByByte[p.Letter].IsConsonant ? 1 : 0) ?? 0;
        int jok = letters?.Sum(p => tileConfig.LettersByByte[p.Letter].IsJoker ? 1 : 0) ?? 0;

        return vow > 0 && con > 0;
    }

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
