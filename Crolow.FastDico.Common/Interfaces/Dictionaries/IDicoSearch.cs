namespace Crolow.FastDico.Common.Interfaces.Dictionaries;


/// <summary>
/// Defines methods and properties to search a letter-based dictionary, exposing the root letter node and operations for
/// length-, pattern-, prefix-, suffix- and letter-based queries.   
/// </summary>
public interface IDicoSearch
{
    /// <summary>
    /// Gets the root node of the letter tree.  
    /// </summary>
    ILetterNode Root { get; }

    /// <summary>
    /// Searches for all words within the specified length range.
    /// </summary>
    /// <param name="minLength">The minimum length of words to include.</param>
    /// <param name="maxLength">The maximum length of words to include.</param>
    /// <returns>A list of matching words</returns>
    List<string> SearchAllWords(int minLength, int maxLength);

    /// <summary>
    /// Searches entries that match the specified pattern and returns matching strings. 
    /// </summary>
    /// <param name="pattern">The pattern to match against items.</param>
    /// <param name="minLength">Minimum length (inclusive) of results; set to int.MinValue to disable the lower bound.</param>
    /// <param name="maxLength">Maximum length (inclusive) of results; set to int.MaxValue to disable the upper bound.</param>
    /// <returns>A list of strings that match the pattern and satisfy the length constraints; an empty list if no matches are
    /// found.</returns>
    List<string> SearchByPattern(string pattern, int minLength = int.MinValue, int maxLength = int.MaxValue);
    
    /// <summary>
    /// Finds entries that start with the specified prefix and returns matching strings.    
    /// </summary>
    /// <param name="prefix">The prefix to match at the start of each result.</param>
    /// <param name="maxLength">Maximum allowed length of returned strings; defaults to int.MaxValue for no length restriction.</param>
    /// <returns>A list of matching strings; empty if no matches are found.</returns>
    List<string> SearchByPrefix(string prefix, int maxLength = int.MaxValue);

    /// <summary>
    /// Finds strings that end with the specified suffix and whose length is less than or equal to maxLength.   
    /// </summary>
    /// <param name="suffix">The suffix to match at the end of each string.</param>
    /// <param name="maxLength">The maximum allowed length of returned strings; defaults to int.MaxValue.</param>
    /// <returns>A list of matching strings that end with the specified suffix and do not exceed maxLength.</returns>
    List<string> SearchBySuffix(string suffix, int maxLength = int.MaxValue);

    /// <summary>
    /// Finds all words that can be constructed  the specified letters.     
    /// </summary>
    /// <param name="pattern">The string of letters used to construct candidate words; may contain repeated characters.</param>
    /// <returns>A list of words that can be formed from the provided letters.</returns>
    List<string> FindAllWordsFromLetters(string pattern);

    /// <summary>
    /// Finds all words that contain the specified letters. 
    /// </summary>
    /// <param name="pattern">The letters to search for within each word.</param>
    /// <returns>A list of matching words; empty if no matches are found.</returns>
    List<string> FindAllWordsContainingLetters(string pattern);

    /// <summary>
    /// Determines whether the specified word exists.   
    /// </summary>
    /// <param name="word">The word to locate.</param>
    /// <returns>true if the word is found; otherwise, false.</returns>
    bool SearchWord(string word);

    /// <summary>
    /// Searches the current data for an exact occurrence of the specified byte sequence.   
    /// </summary>
    /// <param name="word">The byte sequence to locate.</param>
    /// <returns>True if the sequence is found; otherwise, false.</returns>
    bool SearchWord(List<byte> word);
}
