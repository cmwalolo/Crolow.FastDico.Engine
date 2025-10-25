namespace Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi
{
    public interface ITileConfig
    {
        string Char { get; set; }
        bool IsConsonant { get; set; }
        bool IsJoker { get; set; }
        bool IsVowel { get; set; }
        byte Letter { get; set; }
        int Points { get; set; }
        int TotalLetters { get; set; }
    }
}