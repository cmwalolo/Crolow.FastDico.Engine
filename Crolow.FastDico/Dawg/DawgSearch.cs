using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Utils;

namespace Crolow.FastDico.Dawg;

/// <summary>
/// Provides searching capabilities on a directed acyclic word graph (DAWG).
/// The class exposes methods to search for full words, prefixes, suffixes, patterns
/// and to find words based on available letter tiles (including jokers).
/// </summary>
public class DawgSearch : IDicoSearch
{
    /// <summary>
    /// Root node of the DAWG used for all search operations.
    /// </summary>
    public ILetterNode Root { get; private set; }

    /// <summary>
    /// Utilities for handling tile/letter conversions and configuration access.
    /// </summary>
    protected ITilesUtils tilesUtils;

    /// <summary>
    /// Initializes a new instance of <see cref="DawgSearch"/>.
    /// </summary>
    /// <param name="root">The root node of the DAWG.</param>
    /// <param name="tilesUtils">Tile utilities used to convert between characters and internal byte representation.</param>
    public DawgSearch(ILetterNode root, ITilesUtils tilesUtils)
    {
        Root = root;
        this.tilesUtils = tilesUtils;
    }

    #region Search functions

    /// <summary>
    /// Returns all words stored in the DAWG that have a length between <paramref name="minLength"/> and <paramref name="maxLength"/>.
    /// </summary>
    /// <param name="minLength">Minimum word length (inclusive).</param>
    /// <param name="maxLength">Maximum word length (inclusive).</param>
    /// <returns>List of words matching the length constraints.</returns>
    public List<string> SearchAllWords(int minLength, int maxLength)
    {
        var results = new List<string>();
        SearchAllWordsRecursive(Root, new List<byte>(), results, minLength, maxLength);
        return results;
    }
    /// <summary>
    /// Determines whether the specified <paramref name="word"/> exists in the DAWG.
    /// </summary>
    /// <param name="word">The word to search for. Case is ignored.</param>
    /// <returns><c>true</c> if the word exists; otherwise <c>false</c>.</returns>
    public bool SearchWord(string word)
    {
        List<byte> byteWord = tilesUtils.ConvertWordToBytes(word.ToUpper());
        return SearchWord(byteWord);
    }

    /// <summary>
    /// Determines whether the specified byte-encoded word exists in the DAWG.
    /// </summary>
    /// <param name="byteWord">A list of bytes representing the letters of the word using the internal tile mapping.</param>
    /// <returns><c>true</c> if the word exists; otherwise <c>false</c>.</returns>
    public bool SearchWord(List<byte> byteWord)
    {
        var currentNode = Root;
        foreach (var byteVal in byteWord)
        {
            var target = currentNode.Children.Where(p => p.Letter == byteVal);
            if (!target.Any())
            {
                return false; // Word not found
            }
            currentNode = target.First();
        }

        return currentNode.IsEnd; // Return true if the node is terminal
    }

    // Search for all words beginning with a given prefix
    /// <summary>
    /// Finds all words that start with the given <paramref name="prefix"/>.
    /// </summary>
    /// <param name="prefix">Prefix to search for. Case is ignored.</param>
    /// <param name="maxLength">Optional maximum word length to include in results.</param>
    /// <returns>List of words that begin with the provided prefix.</returns>
    public List<string> SearchByPrefix(string prefix, int maxLength = int.MaxValue)
    {
        var currentNode = Root;
        List<byte> bytePrefix = tilesUtils.ConvertWordToBytes(prefix.ToUpper());
        List<string> results = new List<string>();

        // Traverse to the node that represents the end of the prefix
        foreach (var byteVal in bytePrefix)
        {
            var target = currentNode.Children.Where(p => p.Letter == byteVal);
            if (!target.Any())
            {
                return results; // No words found with this prefix
            }
            currentNode = target.First();
        }

        // Once we reach the prefix node, gather all words that start with this prefix
        FindAllWordsFromNode(currentNode, bytePrefix, results);
        return results;
    }

    /// <summary>
    /// Finds all words that end with the given suffix.
    /// This is implemented by translating the suffix into a pattern using a leading '*' wildcard.
    /// </summary>
    /// <param name="prefix">Suffix to search for (name kept for historical reasons).</param>
    /// <param name="maxLength">Optional maximum word length (currently not applied to the pattern search overload).</param>
    /// <returns>List of words that end with the provided suffix.</returns>
    public List<string> SearchBySuffix(string prefix, int maxLength = int.MaxValue)
    {
        return SearchByPattern("*" + prefix.ToUpper());
    }

    /// <summary>
    /// Helper method: starting from <paramref name="node"/>, collects all complete words by appending children
    /// to the provided <paramref name="prefix"/> byte list and pushing completed words into <paramref name="results"/>.
    /// </summary>
    /// <param name="node">Starting node to traverse.</param>
    /// <param name="prefix">Current accumulated byte sequence representing the prefix (modified in-place).</param>
    /// <param name="results">List where discovered words are added.</param>
    private void FindAllWordsFromNode(ILetterNode node, List<byte> prefix, List<string> results)
    {
        if (node.IsEnd)
        {
            results.Add(tilesUtils.ConvertBytesToWord(prefix));
        }

        foreach (var child in node.Children)
        {
            prefix.Add(child.Letter);
            FindAllWordsFromNode(child, prefix, results);
            prefix.RemoveAt(prefix.Count - 1); // backtrack
        }
    }
    /// <summary>
    /// Searches the DAWG for words matching the given <paramref name="pattern"/>. Supports two wildcards:
    /// '*' matches zero or more characters and '?' matches exactly one character.
    /// </summary>
    /// <param name="pattern">Pattern to match. Use '*' and '?' as wildcards. Case is ignored.</param>
    /// <param name="minLength">Minimum word length to include in results.</param>
    /// <param name="maxLength">Maximum word length to include in results.</param>
    /// <returns>List of words matching the provided pattern and length constraints.</returns>
    public List<string> SearchByPattern(string pattern, int minLength = int.MinValue, int maxLength = int.MaxValue)
    {
        // Convert the pattern into bytes
        List<byte> bytePattern = ConvertPatternToBytes(pattern.ToUpper());
        List<string> results = new List<string>();
        SearchByPatternRecursive(Root, bytePattern, 0, new List<byte>(), results, minLength, maxLength);
        return results;
    }

    /// <summary>
    /// Recursively traverses the DAWG collecting words that respect the provided length bounds.
    /// </summary>
    /// <param name="currentNode">Current node in traversal.</param>
    /// <param name="word">Accumulated byte sequence for the current word.</param>
    /// <param name="results">List to which matching words will be added.</param>
    /// <param name="minLength">Minimum word length.</param>
    /// <param name="maxLength">Maximum word length.</param>
    /// <returns>Always returns <c>false</c>. (Kept for historical compatibility.)</returns>
    private bool SearchAllWordsRecursive(ILetterNode currentNode, List<byte> word, List<string> results, int minLength, int maxLength)
    {
        if (currentNode.IsEnd)
        {
            if (word.Count >= minLength && word.Count <= maxLength)
            {
                results.Add(tilesUtils.ConvertBytesToWord(word));
            }
        }

        if (word.Count > maxLength)
        {
            return false;
        }

        // Traverse children to find the matching letter
        foreach (var child in currentNode.Children)
        {
            word.Add(child.Letter);
            SearchAllWordsRecursive(child, word, results, minLength, maxLength);
            word.RemoveAt(word.Count - 1); // Backtrack     
        }

        return false;
    }
    /// <summary>
    /// Recursive engine that matches a byte-encoded pattern against the DAWG.
    /// Handles '*' (WildcardByte) and '?' (JokerByte) semantics as produced by <see cref="ConvertPatternToBytes"/>.
    /// </summary>
    /// <param name="currentNode">Node currently being inspected.</param>
    /// <param name="bytePattern">Byte-encoded pattern where special wildcard bytes are used.</param>
    /// <param name="patternIndex">Current index in the pattern being matched.</param>
    /// <param name="currentWord">Bytes accumulated for the current candidate word.</param>
    /// <param name="results">List to which matching words are added.</param>
    /// <param name="minLength">Minimum word length to accept.</param>
    /// <param name="maxLength">Maximum word length to accept.</param>
    private void SearchByPatternRecursive(ILetterNode currentNode, List<byte> bytePattern, int patternIndex, List<byte> currentWord, List<string> results, int minLength, int maxLength)
    {
        // Base case: Reached the end of the pattern
        if (patternIndex == bytePattern.Count)
        {
            if (currentNode.IsEnd)
            {
                if (currentWord.Count >= minLength && currentWord.Count <= maxLength)
                {
                    // Convert the current word to string and add to results    
                    results.Add(tilesUtils.ConvertBytesToWord(currentWord));
                }
            }
            return;
        }

        if (currentWord.Count > maxLength)
            return;

        byte currentByte = bytePattern[patternIndex];

        if (currentByte == TilesUtils.WildcardByte) // '*' wildcard
        {
            // Match zero or more characters
            // First, try skipping the '*'
            SearchByPatternRecursive(currentNode, bytePattern, patternIndex + 1, currentWord, results, minLength, maxLength);

            // Then, try matching one or more characters
            foreach (var child in currentNode.Children)
            {
                currentWord.Add(child.Letter);
                SearchByPatternRecursive(child, bytePattern, patternIndex, currentWord, results, minLength, maxLength);
                currentWord.RemoveAt(currentWord.Count - 1); // Backtrack
            }
        }
        else if (currentByte == TilesUtils.JokerByte) // '?' wildcard
        {
            // Match exactly one character
            foreach (var child in currentNode.Children)
            {
                currentWord.Add(child.Letter);
                SearchByPatternRecursive(child, bytePattern, patternIndex + 1, currentWord, results, minLength, maxLength);
                currentWord.RemoveAt(currentWord.Count - 1); // Backtrack
            }
        }
        else
        {
            // Match the exact character
            var nextNode = currentNode.Children.Where(p => p.Letter == currentByte);
            if (nextNode.Any())
            {
                currentWord.Add(currentByte);
                SearchByPatternRecursive(nextNode.First(), bytePattern, patternIndex + 1, currentWord, results, minLength, maxLength);
                currentWord.RemoveAt(currentWord.Count - 1); // Backtrack
            }
        }
    }

    /// <summary>
    /// Converts a pattern string into the internal byte representation used by the DAWG search routines.
    /// '*' is mapped to <see cref="TilesUtils.WildcardByte"/> and '?' to <see cref="TilesUtils.JokerByte"/>.
    /// Other characters are mapped via the <see cref="tilesUtils"/>'s configuration.
    /// </summary>
    /// <param name="pattern">Pattern string to convert; should be already upper-cased when passed.</param>
    /// <returns>List of bytes representing the pattern.</returns>
    private List<byte> ConvertPatternToBytes(string pattern)
    {
        List<byte> bytePattern = new List<byte>();
        foreach (char c in pattern)
        {
            if (c == '*')
                bytePattern.Add(TilesUtils.WildcardByte); // '*' wildcard
            else if (c == '?')
                bytePattern.Add(TilesUtils.JokerByte); // '?' wildcard
            else
                bytePattern.Add(tilesUtils.configuration.LettersByChar[c].Letter);
        }
        return bytePattern;
    }

    // Function 1: Find all words that can be formed using exactly the given letters
    /// <summary>
    /// Finds all words that can be formed using exactly the provided letters (tiles).
    /// Jokers may be provided in the input pattern and will be used as wildcards.
    /// </summary>
    /// <param name="pattern">String of available letters (and optionally joker characters) to use to form words.</param>
    /// <returns>List of words that can be formed using exactly the provided letters.</returns>
    public List<string> FindAllWordsFromLetters(string pattern)
    {
        var letters = tilesUtils.ConvertWordToBytes(pattern.ToUpper());

        var results = new List<string>();
        FindWordsUsingLetters(Root, letters, new List<byte>(), new List<byte>(), results, true);
        return results;
    }

    // Function 2: Find all words that contain at least one of the given letters
    /// <summary>
    /// Finds all words that contain at least one of the provided letters.
    /// Useful to search for words that include any of a set of tiles.
    /// </summary>
    /// <param name="pattern">String of letters where at least one must be present in matching words.</param>
    /// <returns>List of matching words that contain at least one of the provided letters.</returns>
    public List<string> FindAllWordsContainingLetters(string pattern)
    {
        var letters = tilesUtils.ConvertWordToBytes(pattern.ToUpper());
        var results = new List<string>();
        FindWordsUsingLetters(Root, letters, new List<byte>(), new List<byte>(), results, false);
        return results;
    }

    /// <summary>
    /// Recursive helper used by the letter-based search functions. Traverses the DAWG and consumes available letters
    /// (and jokers) to build candidate words. When <paramref name="requireExactMatch"/> is <c>true</c>, only words
    /// consuming all provided letters are returned.
    /// </summary>
    /// <param name="currentNode">Node to traverse.</param>
    /// <param name="availableLetters">Remaining available letters (modified in-place during traversal).</param>
    /// <param name="currentWord">Accumulated bytes for the current word being built.</param>
    /// <param name="currentJokers">Parallel list indicating which characters were produced by jokers (1) or real letters (0).</param>
    /// <param name="results">List where formatted display words are added.</param>
    /// <param name="requireExactMatch">If <c>true</c>, only words that consume all available letters are included.</param>
    private void FindWordsUsingLetters(
        ILetterNode currentNode,
        List<byte> availableLetters,
        List<byte> currentWord,
        List<byte> currentJokers,
        List<string> results,
        bool requireExactMatch)
    {
        foreach (var child in currentNode.Children)
        {
            if (availableLetters.Contains(child.Letter) || availableLetters.Contains(TilesUtils.JokerByte))
            {
                bool isJoker = false;
                // Use the letter, removing it from available letters
                currentWord.Add(child.Letter);

                if (availableLetters.Contains(child.Letter))
                {
                    currentJokers.Add(0);
                    availableLetters.Remove(child.Letter);
                }
                else if (availableLetters.Contains(TilesUtils.JokerByte))
                {
                    isJoker = true;
                    currentJokers.Add(1);
                    availableLetters.Remove(TilesUtils.JokerByte);
                }

                if (child.IsEnd)
                {
                    if (!requireExactMatch || availableLetters.Count == 0)
                    {
                        results.Add(tilesUtils.ConvertBytesToWordForDisplay(currentWord, currentJokers));
                    }
                }

                FindWordsUsingLetters(child, availableLetters, currentWord, currentJokers, results, requireExactMatch);

                // Backtrack to restore statev
                if (!isJoker)
                {
                    availableLetters.Add(child.Letter);
                }
                else
                {
                    availableLetters.Add(TilesUtils.JokerByte);
                }

                currentWord.RemoveAt(currentWord.Count - 1);
                currentJokers.RemoveAt(currentJokers.Count - 1);
                isJoker = false;
            }
        }
    }
    #endregion
}


