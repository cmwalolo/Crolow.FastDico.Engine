using Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi;

namespace Crolow.FastDico.Common.Interfaces.Dictionaries
{
    public interface ITileConfiguration
    {
        Dictionary<byte, ITileConfig> LettersByByte { get; set; }
        Dictionary<char, ITileConfig> LettersByChar { get; set; }
        string Name { get; set; }
    }
}