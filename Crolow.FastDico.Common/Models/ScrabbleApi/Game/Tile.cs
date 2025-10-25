using Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace Crolow.FastDico.Common.Models.ScrabbleApi.Game;

// *******************************
// Do never Move this to a class ! 
// *******************************

public struct Tile
{
    public Tile()
    {
        IsEmpty = true;
    }

    public Tile(ITileConfig tile, Square parent)
    {
        IsEmpty = false;
        Letter = tile.Letter;
        Points = tile.Points;
        IsJoker = tile.IsJoker;
    }

    public Tile(Tile tile, Square parent)
    {
        IsEmpty = false;
        Letter = tile.Letter;
        Points = tile.Points;
        IsJoker = tile.IsJoker;
        Parent = tile.Parent;
        IsJokerReplaced = tile.IsJokerReplaced;
        Source = parent?.Status ?? tile.Source;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is Tile tile)
        {
            return (!tile.IsJoker && !IsJoker && Letter == tile.Letter) || (tile.IsJoker && IsJoker);
        }
        return false;
    }

    public bool IsPLayedJoker { get { return IsJoker || IsJokerReplaced; } }

    [JsonIgnore]
    public Square Parent { get; set; }
    public int PivotPoints { get; set; }
    public int Source { get; set; }

    public byte Letter { get; set; }
    public int Points { get; set; }
    public bool IsJoker { get; set; }
    public bool IsJokerReplaced { get; set; }
    public bool IsEmpty { get; set; }
}
