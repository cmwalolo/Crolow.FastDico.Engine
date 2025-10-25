using Crolow.FastDico.Common.Models.ScrabbleApi.Game;

namespace Crolow.FastDico.ScrabbleApi.Extensions
{
    public static class SquareExtensions
    {
        public static void SetPivot(this Square sq, byte letter, int direction)
        {
            sq.Pivots[direction] = sq.Pivots[direction] | 1u << letter;
        }

        public static void SetPivotPoints(this Square sq, int direction, int points)
        {
            sq.PivotPoints[direction] = points;
        }

        public static void SetPivot(this Square sq, uint letter, int direction)
        {
            sq.Pivots[direction] = letter;
        }

        public static void SetPivotLetters(this Square sq, int letters, int direction)
        {
            sq.PivotLetters[direction] = letters;
        }

        public static bool GetPivot(this Square sq, Tile letter, int direction, byte joker, bool checkJoker = true)
        {
            var c = (letter.IsJoker && checkJoker) ? joker : letter.Letter;
            return (sq.Pivots[direction] & 1u << c) > 0;
        }

        public static bool GetPivot(this Square sq, int direction, byte letter)
        {
            return (sq.Pivots[direction] & 1u << letter) > 0;
        }

        public static uint GetPivot(this Square sq, int direction)
        {
            return sq.Pivots[direction];
        }

        public static int GetPivotLetters(this Square sq, int direction)
        {
            return sq.PivotLetters[direction];
        }

        public static int GetPivotPoints(this Square sq, int direction)
        {
            return sq.PivotPoints[direction];
        }


        public static void ResetPivot(this Square sq, int grid, int points, uint maskValue = uint.MaxValue)
        {
            sq.Pivots[grid] = maskValue;
            sq.PivotPoints[grid] = points;
            sq.PivotLetters[grid] = 0;
        }


        public static int GetPivotPoint(this Square sq, byte letter, int direction)
        {
            return sq.PivotPoints[direction];
        }

    }
}
