using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Search;
using static Crolow.FastDico.Search.WordResults;

namespace Crolow.FastDico.Utils;

public class WordTilesUtils
{
    ITilesUtils tilesUtils;
    ITileConfiguration configuration;

    public WordTilesUtils(ITilesUtils tilesUtils)
    {
        this.tilesUtils = tilesUtils;
        configuration = tilesUtils.configuration;
    }

    public string ConvertBytesToWordForDisplay(WordResults.Word word)
    {
        return ConvertWordTilesToWord(word);
    }

    public string ConvertBytesToWordByStatus(WordResults.Word word, int status)
    {
        var tiles = word.Tiles.Where(p => p.Status == status).OrderBy(p => p.Letter).ToArray();
        return ConvertWordTilesToWord(tiles);
    }

    public List<WordResults.ResultTile> ConvertWordToBytes(string word, string optional)
    {
        List<WordResults.ResultTile> byteArray = new List<WordResults.ResultTile>();
        foreach (var letter in word)
        {
            switch (letter)
            {
                case '?':
                    byteArray.Add(new WordResults.ResultTile(TilesUtils.JokerByte, true, 0));
                    break;
                case '*':
                    byteArray.Add(new WordResults.ResultTile(TilesUtils.WildcardByte, true, 0));
                    break;
                default:
                    byteArray.Add(new WordResults.ResultTile((byte)(letter - 'A'), false, 0));
                    break;
            }
        }

        foreach (var letter in optional)
        {
            switch (letter)
            {
                case '?':
                    byteArray.Add(new WordResults.ResultTile(TilesUtils.JokerByte, true, 1));
                    break;
                default:
                    byteArray.Add(new WordResults.ResultTile((byte)(letter - 'A'), false, 1));
                    break;
            }
        }
        return byteArray;
    }

    private string ConvertWordTilesToWord(WordResults.Word word)
    {
        char[] wordChars = new char[word.Tiles.Count];
        for (int i = 0; i < word.Tiles.Count; i++)
        {
            var t = word.Tiles[i];
            byte b = t.Letter;
            bool isJoker = t.IsJoker;

            wordChars[i] = b == TilesUtils.PivotByte ? '#' : configuration.LettersByByte[b].Char[0];
            if (isJoker)
            {
                wordChars[i] = char.ToLower(wordChars[i]);
            }
        }
        return new string(wordChars);
    }

    public string ConvertWordTilesToWord(ResultTile[] tiles)
    {
        char[] wordChars = new char[tiles.Length];
        for (int i = 0; i < tiles.Length; i++)
        {
            var t = tiles[i];
            byte b = t.Letter;
            bool isJoker = t.IsJoker;

            wordChars[i] = b == TilesUtils.PivotByte ? '#' : configuration.LettersByByte[b].Char[0];
            if (isJoker)
            {
                wordChars[i] = char.ToLower(wordChars[i]);
            }
        }
        return new string(wordChars);
    }
}
