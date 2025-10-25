using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi;

namespace Crolow.FastDico.ScrabbleApi.Config
{
    public class TileConfiguration : ITileConfiguration
    {
        public string Name { get; set; }
        public Dictionary<byte, ITileConfig> LettersByByte { get; set; } = new Dictionary<byte, ITileConfig>();
        public Dictionary<char, ITileConfig> LettersByChar { get; set; } = new Dictionary<char, ITileConfig>();
    }

}
