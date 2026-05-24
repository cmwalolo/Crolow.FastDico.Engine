using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Common.Models.ScrabbleApi.Game;

namespace Crolow.FastDico.Utils;

public class TilesUtils : ITilesUtils
{
    public ITilesConfiguration Configuration { get; set; }

    public const byte IsEnd = 1;
    public const byte PivotByte = 255;          // "#"
    public const byte WildcardByte = 253;       // "*"
    public const byte JokerByte = 252;          // "?" 

    /// <summary>
    /// Initializes a new instance of TilesUtils using the provided tile configuration. 
    /// </summary>
    /// <param name="config">The configuration used by the tiles utility.</param>
    public TilesUtils(ITilesConfiguration config)
    {
        Configuration = config;
    }

    /// <summary>
    /// Converts a word to a sequence of tile bytes, mapping '#' to TilesUtils.PivotByte, '?' to TilesUtils.JokerByte,
    /// '*' to TilesUtils.WildcardByte, and letters to their configured byte values.    
    /// </summary>
    /// <remarks>Exceptions thrown during conversion are suppressed, so a partial result may be returned if a
    /// lookup or other error occurs.</remarks>
    /// <param name="word">The input word containing letters and optional special characters ('#', '?', '*').</param>
    /// <returns>A list of bytes representing the word as tile bytes; if an exception occurs the method returns the bytes parsed
    /// before the error.</returns>
    public List<byte> ConvertWordToBytes(string word)
    {
        List<byte> byteArray = new List<byte>();
        try
        {
            foreach (var letter in word)
            {
                switch (letter)
                {
                    case '#':
                        byteArray.Add(TilesUtils.PivotByte);
                        break;
                    case '?':
                        byteArray.Add(TilesUtils.JokerByte);
                        break;
                    case '*':
                        byteArray.Add(TilesUtils.WildcardByte);
                        break;
                    default:
                        byteArray.Add(Configuration.LettersByChar[letter].Letter);
                        break;
                }
            }
        }
        catch (Exception)
        {
            ;
        }
        return byteArray;
    }

    /// <summary>
    /// Create a string from a list of bytes by mapping each byte to a character: PivotByte maps to '#',
    /// TilesUtils.JokerByte maps to '?', otherwise use the first character of configuration.LettersByByte for that
    /// byte.   
    /// </summary>
    /// <remarks>Assumes byteArray is non-null and each byte is a valid index into
    /// configuration.LettersByByte; no null or bounds validation is performed.</remarks>
    /// <param name="byteArray">List<byte> of tile codes to convert into characters.</param>
    /// <returns>A string whose length equals byteArray.Count, containing the mapped characters for each byte.</returns>
    public string ConvertBytesToWord(List<byte> byteArray)
    {
        char[] wordChars = new char[byteArray.Count];
        for (int i = 0; i < byteArray.Count; i++)
        {
            byte b = byteArray[i];
            wordChars[i] = b == PivotByte ? '#' : (b == TilesUtils.JokerByte ? '?' : Configuration.LettersByByte[b].Char[0]);
        }
        return new string(wordChars);
    }

    /// <summary>
    /// Converts an ordered list of Tile objects into a word string by mapping each tile to a character.    
    /// </summary>
    /// <remarks>Uses configuration.LettersByByte to map tile bytes to characters. Preserves tile order.
    /// Returns an empty string for an empty list. Assumes LettersByByte contains a mapping for every tile
    /// letter.</remarks>
    /// <param name="m">Ordered list of Tile objects to convert.</param>
    /// <returns>A string composed from the tiles' characters: '#' for the pivot byte, '?' for a played joker whose letter equals
    /// JokerByte, a lowercase mapped letter for other played jokers, or the mapped character from
    /// configuration.LettersByByte otherwise.</returns>
    public string ConvertTilesToWord(List<Tile> m)
    {
        char[] wordChars = new char[m.Count];
        for (int i = 0; i < m.Count; i++)
        {
            var c = Configuration.LettersByByte[m[i].Letter].Char;
            wordChars[i] = (char)(m[i].Letter == PivotByte ? '#' : (m[i].IsPLayedJoker ? (m[i].Letter == JokerByte ? '?' : char.ToLower(c[0])) : c[0]));
        }
        return new string(wordChars);
    }

    /// <summary>
    /// Converts a sequence of bytes into a display word by mapping each byte to a character using
    /// configuration.LettersByByte.    
    /// </summary>
    /// <remarks>Uses the first character of configuration.LettersByByte[byte].Char. Expects jokers.Count to
    /// match byteArray.Count when jokers is provided.</remarks>
    /// <param name="byteArray">List of bytes representing letters; each byte is mapped via configuration.LettersByByte and the first character
    /// of its Char value is used.</param>
    /// <param name="jokers">Optional per-position flags. If null, bytes equal to JokerByte are rendered as '?'. If provided, a value of 1 at
    /// a position makes the corresponding character lowercase.</param>
    /// <returns>A string composed of the mapped characters in the same order as the input bytes.</returns>
    public string ConvertBytesToWordForDisplay(List<byte> byteArray, List<byte> jokers = null)
    {
        char[] wordChars = new char[byteArray.Count];
        if (jokers == null)
        {
            for (int i = 0; i < byteArray.Count; i++)
            {
                byte b = byteArray[i];
                wordChars[i] = b == JokerByte ? '?' : Configuration.LettersByByte[b].Char[0];
            }
        }
        else
        {
            for (int i = 0; i < byteArray.Count; i++)
            {
                byte b = byteArray[i];
                var c = Configuration.LettersByByte[b].Char;
                wordChars[i] = jokers[i] == 1 ? char.ToLower(c[0]) : c[0];
            }
        }
        return new string(wordChars);
    }
}
