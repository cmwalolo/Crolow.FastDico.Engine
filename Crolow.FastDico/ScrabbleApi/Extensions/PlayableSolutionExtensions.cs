using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi;
using Crolow.FastDico.Utils;
using Kalow.Apps.Common.Utils;

namespace Crolow.FastDico.ScrabbleApi.Extensions;

public static class PlayableSolutionExtensions
{
    public static bool InvariantEquals(string str1, string str2)
    {
        return string.Equals(str1, str2, StringComparison.InvariantCulture);
    }

    public static bool IsEqual(this PlayableSolution s, IPlayableSolutionModel t, ITilesUtils utils, bool reverse)
    {
        var p = new Position(s.Position);
        if (reverse && p.Direction == 1)
        {
            var i = p.X;
            p.X = p.Y;
            p.Y = i;

        }
        return InvariantEquals(s.GetWord(utils, true), t.Word)
             && p.GetPosition().Equals(t.Position);
    }


    public static void AddTile(this PlayableSolution s, Tile tile, Square sq)
    {
        tile.Parent = sq;
        s.Tiles.Add(tile);
    }

    public static Tile RemoveTile(this PlayableSolution s)
    {
        var t = s.Tiles[s.Tiles.Count - 1];
        s.Tiles.RemoveAt(s.Tiles.Count - 1);
        return t;
    }

    public static void SetPivot(this PlayableSolution s)
    {
        s.Pivot = s.Tiles.Count;
    }

    public static void RemovePivot(this PlayableSolution s)
    {
        s.Pivot = 0;
    }

    public static string GetPosition(this PlayableSolution s)
    {
        if (s.Position.Direction == 0)
        {
            return $"{new char[] { (char)(64 + s.Position.Y) }[0]}{s.Position.X}";
        }

        return $"{s.Position.X}{new char[] { (char)(64 + s.Position.Y) }[0]}";
    }


    /// <summary>
    /// We reset the tiles and create of them to keep the status
    /// </summary>
    public static void FinalizeRound(this PlayableSolution s)
    {
        if (!s.Finalized)
        {
            var l = s.Tiles.Take(s.Pivot);
            var m = s.Tiles.Skip(s.Pivot).ToList();
            if (s.Pivot != 0)
            {
                m.Reverse();
            }
            if (l.Count() > 0)
            {
                m.AddRange(l);
            }
            s.Tiles = m.Select(t => new Tile(t, t.Parent)).ToList();
            s.Pivot = 0;

            // If we played vertically, we reset the position
            // To the transposed coordinate
            if (s.Position.Direction == 1)
            {
                s.Position = new Position(s.Position.Y, s.Position.X, 1);
            }
            s.Finalized = true;
        }
    }
    public static void DebugRound(this PlayableSolution s, ITilesUtils utils, string message)
    {
        string res = s.GetWord(utils, true);
        string resRaw = s.GetWord(utils, false);

        var txt = $"{message} : {res} {s.Points} : {s.GetPosition()} - {resRaw}";
        BufferedConsole.WriteLine(txt);
    }

    public static string GetWord(this PlayableSolution s, ITilesUtils utils, bool reorder = true)
    {
        if (reorder)
        {
            var l = s.Tiles.Take(s.Pivot);
            var m = s.Tiles.Skip(s.Pivot).ToList();
            if (s.Pivot != 0)
            {
                m.Reverse();
            }
            if (l.Count() > 0)
            {
                m.AddRange(l);
            }
            return utils.ConvertTilesToWord(m);
        }
        else
        {
            var m = s.Tiles;
            return utils.ConvertTilesToWord(m);
        }
    }

    public static string ToString(this PlayableSolution s, ITilesUtils utils)
    {
        return s.GetWord(utils, true);
    }

}
