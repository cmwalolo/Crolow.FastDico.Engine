using Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi;

namespace Crolow.FastDico.Common.Interfaces.Dictionaries
{
    public interface ITilesConfiguration
    {
        Dictionary<byte, ITileConfiguration> LettersByByte { get; set; }
        Dictionary<char, ITileConfiguration> LettersByChar { get; set; }
        string Name { get; set; }
    }
}