using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Search;
using static Crolow.FastDico.Search.WordResults;

namespace Crolow.FastDico.Utils;

/// <summary>
/// Converts extended word-result tiles to and from display text and byte-search input.
/// </summary>
public class WordTilesUtils
{
    ITilesUtils tilesUtils;
    ITilesConfiguration configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="WordTilesUtils"/> class.
    /// </summary>
    /// <param name="tilesUtils">Tile utility that supplies the active tile configuration.</param>
    public WordTilesUtils(ITilesUtils tilesUtils)
    {
        this.tilesUtils = tilesUtils;
        configuration = tilesUtils.Configuration;
    }

    /// <summary>
    /// Converts a word result to its display representation.
    /// </summary>
    /// <param name="word">Word result to convert.</param>
    /// <returns>Display text for the word, with joker letters lower-cased.</returns>
    public string ConvertBytesToWordForDisplay(WordResults.Word word)
    {
        return ConvertWordTilesToWord(word);
    }

    /// <summary>
    /// Converts only tiles with the requested status to display text.
    /// </summary>
    /// <param name="word">Word result containing the tiles to filter.</param>
    /// <param name="status">Status value used to select tiles.</param>
    /// <returns>Display text built from the matching tiles.</returns>
    public string ConvertBytesToWordByStatus(WordResults.Word word, int status)
    {
        var tiles = word.Tiles.Where(p => p.Status == status).OrderBy(p => p.Letter).ToArray();
        return ConvertWordTilesToWord(tiles);
    }

    /// <summary>
    /// Converts mandatory and optional word fragments to search result tiles.
    /// </summary>
    /// <param name="word">Mandatory pattern letters, where <c>?</c> is a joker and <c>*</c> is a wildcard.</param>
    /// <param name="optional">Optional letters to append with status <c>1</c>.</param>
    /// <returns>Search tiles representing the mandatory and optional fragments.</returns>
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

    /// <summary>
    /// Converts the tiles of a word result to display text.
    /// </summary>
    /// <param name="word">Word result to convert.</param>
    /// <returns>Display text for the provided word.</returns>
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

    /// <summary>
    /// Converts an array of result tiles to display text.
    /// </summary>
    /// <param name="tiles">Tiles to convert.</param>
    /// <returns>Display text for the provided tiles.</returns>
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
