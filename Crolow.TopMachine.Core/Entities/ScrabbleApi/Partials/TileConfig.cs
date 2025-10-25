using Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi;

namespace Crolow.FastDico.Common.Models.ScrabbleApi.Entities.Partials;

public class TileConfig : ITileConfig
{
    public TileConfig()
    {
    }

    public string Char { get; set; }
    public byte Letter { get; set; }
    public bool IsVowel { get; set; }
    public bool IsConsonant { get; set; }
    public int TotalLetters { get; set; }
    public int Points { get; set; }
    public bool IsJoker { get; set; }
}
