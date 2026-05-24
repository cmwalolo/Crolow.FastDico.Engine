using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi;

namespace Crolow.FastDico.ScrabbleApi.Config
{
    public class TilesConfiguration : ITilesConfiguration
    {
        public string Name { get; set; }
        public Dictionary<byte, ITileConfiguration> LettersByByte { get; set; } = new Dictionary<byte, ITileConfiguration>();
        public Dictionary<char, ITileConfiguration> LettersByChar { get; set; } = new Dictionary<char, ITileConfiguration>();
    }

}
