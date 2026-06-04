using Crolow.FastDico.Common.Models.ScrabbleApi.Game;

namespace Crolow.FastDico.ScrabbleApi.Extensions;

/// <summary>
/// Provides helper methods for Scrabble board positions.
/// </summary>
public static class PositionExtensions
{
    /// <summary>
    /// Determines whether one position is greater than another on either axis.
    /// </summary>
    /// <param name="p1">Position to compare.</param>
    /// <param name="p2">Position used as the comparison baseline.</param>
    /// <returns><c>true</c> when <paramref name="p1"/> has a greater X or Y coordinate.</returns>
    public static bool ISGreater(this Position p1, Position p2)
    {
        return p1.X > p2.X || p1.Y > p2.Y;
    }

    /// <summary>
    /// Converts a board position to its human-readable Scrabble coordinate.
    /// </summary>
    /// <param name="p">Position to format.</param>
    /// <returns>A coordinate string whose order depends on the position direction.</returns>
    public static string GetPosition(this Position p)
    {
        if (p.Direction == 0)
        {
            return $"{new char[] { (char)(64 + p.Y) }[0]}{p.X}";
        }

        return $"{p.X}{new char[] { (char)(64 + p.Y) }[0]}";
    }
}
