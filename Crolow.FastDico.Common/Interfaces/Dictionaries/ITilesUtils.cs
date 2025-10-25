using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Common.Models.ScrabbleApi.Game;

namespace Crolow.FastDico.Utils
{
    public interface ITilesUtils
    {
        ITileConfiguration configuration { get; set; }

        string ConvertBytesToWord(List<byte> byteArray);
        string ConvertBytesToWordForDisplay(List<byte> byteArray, List<byte> jokers = null);
        string ConvertTilesToWord(List<Tile> m);

        List<byte> ConvertWordToBytes(string word);
    }
}