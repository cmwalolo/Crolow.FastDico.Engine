using Crolow.FastDico.Common.Models.ScrabbleApi.Game;

namespace Crolow.FastDico.ScrabbleApi.Extensions;

public static class PositionExtensions
{
    public static bool ISGreater(this Position p1, Position p2)
    {
        return p1.X > p2.X || p1.Y > p2.Y;
    }

    public static string GetPosition(this Position p)
    {
        if (p.Direction == 0)
        {
            return $"{new char[] { (char)(64 + p.Y) }[0]}{p.X}";
        }

        return $"{p.X}{new char[] { (char)(64 + p.Y) }[0]}";
    }
}
