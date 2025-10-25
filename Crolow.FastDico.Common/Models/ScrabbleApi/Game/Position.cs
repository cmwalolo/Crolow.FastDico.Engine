namespace Crolow.FastDico.Common.Models.ScrabbleApi.Game;


// Moved the position to a struct, so it can be on the heap
public struct Position : IEquatable<Position>
{
    public int Direction { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public Position(int x, int y, int direction)
    {
        X = x;
        Y = y;
        Direction = direction;
    }

    public Position(Position copy)
    {
        X = copy.X;
        Y = copy.Y;
        Direction = copy.Direction;
    }

    public bool Equals(Position other)
    {
        return X == other.X && Y == other.Y && Direction == other.Direction;
    }
}
