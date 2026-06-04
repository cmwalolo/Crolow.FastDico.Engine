using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Utils;

namespace Crolow.FastDico.GadDag;

/// <summary>
/// Searches words in a GADDAG dictionary using exact, prefix, suffix, pattern, and rack-letter queries.
/// </summary>
public class GadDagSearch : IDicoSearch
{
    /// <summary>
    /// Gets the root node of the GADDAG graph.
    /// </summary>
    public ILetterNode Root { get; private set; }

    /// <summary>
    /// Gets the tile utility used to encode and decode words.
    /// </summary>
    public ITilesUtils tilesUtils { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GadDagSearch"/> class.
    /// </summary>
    /// <param name="root">Root node of the GADDAG graph.</param>
    /// <param name="tilesUtils">Tile utility used for word conversion.</param>
    public GadDagSearch(ILetterNode root, ITilesUtils tilesUtils)
    {
        Root = root;
        this.tilesUtils = tilesUtils;
    }

    /// <summary>
    /// Finds all words whose length falls within the supplied range.
    /// </summary>
    /// <param name="minLength">Minimum accepted word length.</param>
    /// <param name="maxLength">Maximum accepted word length.</param>
    /// <returns>Words found in the dictionary within the requested range.</returns>
    public List<string> SearchAllWords(int minLength, int maxLength)
    {
        var results = new List<string>();
        SearchAllWordsRecursive(Root, new List<byte>(), results, minLength, maxLength);
        return results;
    }

    /// <summary>
    /// Recursively traverses non-pivot nodes and collects terminal words.
    /// </summary>
    /// <param name="currentNode">Current graph node.</param>
    /// <param name="word">Mutable word byte buffer.</param>
    /// <param name="results">Collection that receives matching words.</param>
    /// <param name="minLength">Minimum accepted word length.</param>
    /// <param name="maxLength">Maximum accepted word length.</param>
    /// <returns>Always returns <c>false</c>; results are written to <paramref name="results"/>.</returns>
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
        foreach (var child in currentNode.Children.Where(p => p.Letter != TilesUtils.PivotByte))
        {
            word.Add(child.Letter);
            SearchAllWordsRecursive(child, word, results, minLength, maxLength);
            word.RemoveAt(word.Count - 1); // Backtrack     
        }

        return false;
    }

    /// <summary>
    /// Determines whether the dictionary contains the specified word.
    /// </summary>
    /// <param name="word">Word to search for.</param>
    /// <returns><c>true</c> when the word is present as a terminal graph path.</returns>
    public bool SearchWord(string word)
    {
        var bytes = tilesUtils.ConvertWordToBytes(word.ToUpper());
        return SearchWordRecursive(Root, bytes, 0);
    }

    /// <summary>
    /// Determines whether the dictionary contains the specified byte-encoded word.
    /// </summary>
    /// <param name="word">Word represented as tile bytes.</param>
    /// <returns><c>true</c> when the byte sequence is present as a terminal graph path.</returns>
    public bool SearchWord(List<byte> word)
    {
        return SearchWordRecursive(Root, word, 0);
    }

    /// <summary>
    /// Recursively follows a byte-encoded word through the graph.
    /// </summary>
    /// <param name="currentNode">Current graph node.</param>
    /// <param name="word">Word represented as tile bytes.</param>
    /// <param name="index">Current index in the word.</param>
    /// <returns><c>true</c> when the traversal ends on a terminal node.</returns>
    private bool SearchWordRecursive(ILetterNode currentNode, List<byte> word, int index)
    {
        if (index == word.Count)
        {
            return currentNode.IsEnd;
        }

        byte currentByte = word[index];

        // Traverse children to find the matching letter
        foreach (var child in currentNode.Children)
        {
            if (child.Letter == currentByte) // Pivot handling
            {
                if (SearchWordRecursive(child, word, index + 1))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Finds words that begin with the supplied prefix.
    /// </summary>
    /// <param name="prefix">Prefix to match.</param>
    /// <param name="maxLength">Maximum number of additional letters to explore.</param>
    /// <returns>Words that start with the prefix.</returns>
    public List<string> SearchByPrefix(string prefix, int maxLength = int.MaxValue)
    {
        var bytes = tilesUtils.ConvertWordToBytes(prefix.ToUpper());
        var results = new List<string>();
        var currentNode = Root;

        // Navigate to the prefix node
        foreach (var byteVal in bytes)
        {
            currentNode = currentNode.Children.FirstOrDefault(c => c.Letter == byteVal);
            if (currentNode == null)
                return results; // Prefix not found
        }

        // Collect all words from the prefix node
        SearchPrefixesFromNode(currentNode, bytes, results, maxLength == 0 ? int.MaxValue : maxLength);
        return results;
    }

    /// <summary>
    /// Collects prefix matches reachable from a prefix node.
    /// </summary>
    /// <param name="node">Node at the end of the prefix.</param>
    /// <param name="currentWord">Mutable word byte buffer.</param>
    /// <param name="results">Collection that receives matching words.</param>
    /// <param name="length">Remaining depth to explore.</param>
    private void SearchPrefixesFromNode(ILetterNode node, List<byte> currentWord, List<string> results, int length)
    {
        if (node.IsEnd && length >= 0)
        {
            results.Add(tilesUtils.ConvertBytesToWord(currentWord));
        }

        if (length == 0)
        {
            return;
        }

        foreach (var child in node.Children)
        {
            if (child.Letter != TilesUtils.PivotByte)
            {
                currentWord.Add(child.Letter);
                SearchPrefixesFromNode(child, currentWord, results, length - 1);
                currentWord.RemoveAt(currentWord.Count - 1); // Backtrack
            }
        }
    }


    /// <summary>
    /// Finds words that end with the supplied suffix.
    /// </summary>
    /// <param name="suffix">Suffix to match.</param>
    /// <param name="maxLength">Maximum number of additional letters to explore.</param>
    /// <returns>Words that end with the suffix.</returns>
    public List<string> SearchBySuffix(string suffix, int maxLength = int.MaxValue)
    {
        var patternedSuffix = suffix.ToUpper() + "#";

        var bytes = tilesUtils.ConvertWordToBytes(patternedSuffix);
        var results = new List<string>();
        var currentNode = Root;

        // Navigate to the prefix node
        foreach (var byteVal in bytes)
        {
            currentNode = currentNode.Children.FirstOrDefault(c => c.Letter == byteVal);
            if (currentNode == null)
                return results; // Prefix not found
        }

        // Collect all words from the prefix node
        SearchSuffixesFromNode(currentNode, bytes, new List<byte>(), results, maxLength);
        return results;
    }

    /// <summary>
    /// Collects suffix matches from a GADDAG pivot traversal.
    /// </summary>
    /// <param name="node">Current graph node.</param>
    /// <param name="currentWord">Encoded suffix path including the pivot.</param>
    /// <param name="result">Mutable prefix buffer built in reverse.</param>
    /// <param name="results">Collection that receives matching words.</param>
    /// <param name="length">Remaining depth to explore.</param>
    private void SearchSuffixesFromNode(ILetterNode node, List<byte> currentWord, List<byte> result, List<string> results, int length)
    {
        if (node.IsEnd && length >= 0)
        {
            var newWord = new List<byte>();
            var word = currentWord.Take(currentWord.Count - 1).ToList();
            newWord.AddRange(result);
            newWord.AddRange(word);
            results.Add(tilesUtils.ConvertBytesToWord(newWord));
        }

        foreach (var child in node.Children)
        {
            result.Insert(0, child.Letter);
            SearchSuffixesFromNode(child, currentWord, result, results, length - 1);
            result.RemoveAt(0);
        }
    }

    /// <summary>
    /// Finds words that match a pattern containing optional wildcard characters.
    /// </summary>
    /// <param name="pattern">Pattern to match, where <c>*</c> matches zero or more letters and <c>?</c> matches one letter.</param>
    /// <param name="minLength">Minimum accepted word length.</param>
    /// <param name="maxLength">Maximum accepted word length.</param>
    /// <returns>Words that match the provided pattern and length constraints.</returns>
    public List<string> SearchByPattern(string pattern, int minLength = int.MinValue, int maxLength = int.MaxValue)
    {
        // Convert the pattern into bytes
        List<byte> bytePattern = ConvertPatternToBytes(pattern.ToUpper());
        List<string> results = new List<string>();
        SearchByPatternRecursive(Root, bytePattern, 0, new List<byte>(), results, minLength, maxLength);
        return results;
    }

    /// <summary>
    /// Recursively evaluates a byte pattern against the GADDAG graph.
    /// </summary>
    /// <param name="currentNode">Current graph node.</param>
    /// <param name="bytePattern">Pattern encoded as tile bytes and wildcard bytes.</param>
    /// <param name="patternIndex">Current index in the pattern.</param>
    /// <param name="currentWord">Mutable word byte buffer.</param>
    /// <param name="results">Collection that receives matching words.</param>
    /// <param name="minLength">Minimum accepted word length.</param>
    /// <param name="maxLength">Maximum accepted word length.</param>
    private void SearchByPatternRecursive(ILetterNode currentNode, List<byte> bytePattern, int patternIndex, List<byte> currentWord, List<string> results, int minLength, int maxLength)
    {
        // Base case: Reached the end of the pattern
        if (patternIndex == bytePattern.Count)
        {
            if (currentNode.IsEnd)
            {
                if (currentWord.Count >= minLength && currentWord.Count <= maxLength)
                {
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
                if (child.Letter != TilesUtils.PivotByte)
                {
                    currentWord.Add(child.Letter);
                    SearchByPatternRecursive(child, bytePattern, patternIndex, currentWord, results, minLength, maxLength);
                    currentWord.RemoveAt(currentWord.Count - 1); // Backtrack
                }
            }
        }
        else if (currentByte == TilesUtils.JokerByte) // '?' wildcard
        {
            // Match exactly one character
            foreach (var child in currentNode.Children)
            {
                if (child.Letter != TilesUtils.PivotByte)
                {
                    currentWord.Add(child.Letter);
                    SearchByPatternRecursive(child, bytePattern, patternIndex + 1, currentWord, results, minLength, maxLength);
                    currentWord.RemoveAt(currentWord.Count - 1); // Backtrack
                }
            }
        }
        else
        {
            // Match the exact character
            var nextNode = currentNode.Children.Where(p => p.Letter != TilesUtils.PivotByte && p.Letter == currentByte);
            if (nextNode.Any())
            {
                currentWord.Add(currentByte);
                SearchByPatternRecursive(nextNode.First(), bytePattern, patternIndex + 1, currentWord, results, minLength, maxLength);
                currentWord.RemoveAt(currentWord.Count - 1); // Backtrack
            }
        }
    }

    /// <summary>
    /// Converts a textual search pattern into configured tile bytes and wildcard marker bytes.
    /// </summary>
    /// <param name="pattern">Pattern containing letters, <c>*</c>, or <c>?</c>.</param>
    /// <returns>The byte representation of the pattern.</returns>
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
                bytePattern.Add(tilesUtils.Configuration.LettersByChar[c].Letter);
        }
        return bytePattern;
    }

    /// <summary>
    /// Finds words that can be formed using exactly the provided letters.
    /// </summary>
    /// <param name="pattern">Letters available in the rack, with optional joker markers.</param>
    /// <returns>Words that consume all provided letters.</returns>
    public List<string> FindAllWordsFromLetters(string pattern)
    {
        var letters = tilesUtils.ConvertWordToBytes(pattern.ToUpper());
        var results = new List<string>();

        FindWordsUsingLetters(Root, letters, new List<byte>(), new List<byte>(), results, true);
        return results;
    }

    /// <summary>
    /// Finds words that can be formed from at least some of the provided letters.
    /// </summary>
    /// <param name="pattern">Letters available in the rack, with optional joker markers.</param>
    /// <returns>Words that can be built from the supplied letters.</returns>
    public List<string> FindAllWordsContainingLetters(string pattern)
    {
        var letters = tilesUtils.ConvertWordToBytes(pattern.ToUpper());
        var results = new List<string>();
        FindWordsUsingLetters(Root, letters, new List<byte>(), new List<byte>(), results, false);
        return results;
    }

    /// <summary>
    /// Recursively builds candidate words from a mutable pool of available letters.
    /// </summary>
    /// <param name="currentNode">Current graph node.</param>
    /// <param name="availableLetters">Letters that can still be consumed.</param>
    /// <param name="currentWord">Current candidate word bytes.</param>
    /// <param name="currentJokers">Per-letter joker flags used for display formatting.</param>
    /// <param name="results">Collection that receives matching display words.</param>
    /// <param name="requireExactMatch">Indicates whether all available letters must be consumed.</param>
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
            if (child.Letter != TilesUtils.PivotByte)
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
                    else
                    {
                        continue;
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
    }
}
