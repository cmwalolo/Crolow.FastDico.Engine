using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi;
using Crolow.FastDico.Utils;
using Kalow.Apps.Common.Utils;

namespace Crolow.FastDico.ScrabbleApi.Extensions;

/// <summary>
/// Provides helper methods for manipulating and displaying playable Scrabble solutions.
/// </summary>
public static class PlayableSolutionExtensions
{
    /// <summary>
    /// Compares two strings using invariant-culture comparison.
    /// </summary>
    /// <param name="str1">First string to compare.</param>
    /// <param name="str2">Second string to compare.</param>
    /// <returns><c>true</c> when the strings are equal with invariant-culture rules.</returns>
    public static bool InvariantEquals(string str1, string str2)
    {
        return string.Equals(str1, str2, StringComparison.InvariantCulture);
    }

    /// <summary>
    /// Determines whether a playable solution matches a persisted solution model.
    /// </summary>
    /// <param name="s">Playable solution to compare.</param>
    /// <param name="t">Persisted solution model to compare against.</param>
    /// <param name="utils">Tile utility used to read the solution word.</param>
    /// <param name="reverse">Indicates whether vertical coordinates should be transposed before comparison.</param>
    /// <returns><c>true</c> when the word and position match.</returns>
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


    /// <summary>
    /// Adds a tile to the solution and assigns its parent square.
    /// </summary>
    /// <param name="s">Solution to update.</param>
    /// <param name="tile">Tile to add.</param>
    /// <param name="sq">Board square associated with the tile.</param>
    public static void AddTile(this PlayableSolution s, Tile tile, Square sq)
    {
        tile.Parent = sq;
        s.Tiles.Add(tile);
    }

    /// <summary>
    /// Removes and returns the last tile from the solution.
    /// </summary>
    /// <param name="s">Solution to update.</param>
    /// <returns>The removed tile.</returns>
    public static Tile RemoveTile(this PlayableSolution s)
    {
        var t = s.Tiles[s.Tiles.Count - 1];
        s.Tiles.RemoveAt(s.Tiles.Count - 1);
        return t;
    }

    /// <summary>
    /// Marks the current tile count as the pivot position of the solution.
    /// </summary>
    /// <param name="s">Solution to update.</param>
    public static void SetPivot(this PlayableSolution s)
    {
        s.Pivot = s.Tiles.Count;
    }

    /// <summary>
    /// Clears the pivot position of the solution.
    /// </summary>
    /// <param name="s">Solution to update.</param>
    public static void RemovePivot(this PlayableSolution s)
    {
        s.Pivot = 0;
    }

    /// <summary>
    /// Gets the board coordinate for the solution.
    /// </summary>
    /// <param name="s">Solution whose position is formatted.</param>
    /// <returns>A human-readable board coordinate.</returns>
    public static string GetPosition(this PlayableSolution s)
    {
        if (s.Position.Direction == 0)
        {
            return $"{new char[] { (char)(64 + s.Position.Y) }[0]}{s.Position.X}";
        }

        return $"{s.Position.X}{new char[] { (char)(64 + s.Position.Y) }[0]}";
    }


    /// <summary>
    /// Reorders the solution around its pivot, normalizes vertical coordinates, and marks it as finalized.
    /// </summary>
    /// <param name="s">Solution to finalize.</param>
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

    /// <summary>
    /// Writes debug information for a playable solution.
    /// </summary>
    /// <param name="s">Solution to describe.</param>
    /// <param name="utils">Tile utility used to format words.</param>
    /// <param name="message">Message prefix written before the solution details.</param>
    public static void DebugRound(this PlayableSolution s, ITilesUtils utils, string message)
    {
        string res = s.GetWord(utils, true);
        string resRaw = s.GetWord(utils, false);

        var txt = $"{message} : {res} {s.Points} : {s.GetPosition()} - {resRaw}";
        BufferedConsole.WriteLine(txt);
    }

    /// <summary>
    /// Gets the word represented by a playable solution.
    /// </summary>
    /// <param name="s">Solution to convert.</param>
    /// <param name="utils">Tile utility used to convert tiles to a word.</param>
    /// <param name="reorder">Indicates whether tiles should be reordered around the pivot before conversion.</param>
    /// <returns>The word represented by the solution tiles.</returns>
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

    /// <summary>
    /// Converts a playable solution to its display word.
    /// </summary>
    /// <param name="s">Solution to convert.</param>
    /// <param name="utils">Tile utility used to convert tiles to a word.</param>
    /// <returns>The display word for the solution.</returns>
    public static string ToString(this PlayableSolution s, ITilesUtils utils)
    {
        return s.GetWord(utils, true);
    }

}
