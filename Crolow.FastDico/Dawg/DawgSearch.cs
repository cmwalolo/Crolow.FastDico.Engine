using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Utils;

namespace Crolow.FastDico.Dawg;

/// <summary>
/// Searches words in a DAWG dictionary using exact, prefix, suffix, pattern, and rack-letter queries.
/// </summary>
public class DawgSearch : IDicoSearch
{
    /// <summary>
    /// Gets the root node of the DAWG graph used for searches.
    /// </summary>
    public ILetterNode Root { get; private set; }

    protected ITilesUtils tilesUtils;

    /// <summary>
    /// Initializes a new instance of the <see cref="DawgSearch"/> class.
    /// </summary>
    /// <param name="root">Root node of the dictionary graph to search.</param>
    /// <param name="tilesUtils">Utility used to convert between words and tile bytes.</param>
    public DawgSearch(ILetterNode root, ITilesUtils tilesUtils)
    {
        Root = root;
        this.tilesUtils = tilesUtils;
    }

    #region Search functions


    /// <summary>
    /// Finds all words whose length falls within the supplied range.
    /// </summary>
    /// <param name="minLength">Minimum accepted word length.</param>
    /// <param name="maxLength">Maximum accepted word length.</param>
    /// <returns>Words found in the dictionary within the requested length range.</returns>
    public List<string> SearchAllWords(int minLength, int maxLength)
    {
        var results = new List<string>();
        SearchAllWordsRecursive(Root, new List<byte>(), results, minLength, maxLength);
        return results;
    }

    /// <summary>
    /// Determines whether the dictionary contains the specified word.
    /// </summary>
    /// <param name="word">Word to search for.</param>
    /// <returns><c>true</c> when the word exists as a terminal node; otherwise, <c>false</c>.</returns>
    public bool SearchWord(string word)
    {
        List<byte> byteWord = tilesUtils.ConvertWordToBytes(word.ToUpper());
        return SearchWord(byteWord);
    }

    /// <summary>
    /// Determines whether the dictionary contains the specified byte-encoded word.
    /// </summary>
    /// <param name="byteWord">Word represented as tile bytes.</param>
    /// <returns><c>true</c> when the byte sequence ends on a terminal node; otherwise, <c>false</c>.</returns>
    public bool SearchWord(List<byte> byteWord)
    {
        var currentNode = Root;
        foreach (var byteVal in byteWord)
        {
            var target = currentNode.Children
                .FirstOrDefault(p => p.Letter == byteVal);

            if (target == null)
            {
                return false;
            }

            currentNode = target;
        }
        return currentNode.IsEnd; // Return true if the node is terminal
    }

    /// <summary>
    /// Finds all words that start with the specified prefix.
    /// </summary>
    /// <param name="prefix">Prefix to match.</param>
    /// <param name="maxLength">Maximum word length to return.</param>
    /// <returns>Words that begin with <paramref name="prefix"/>.</returns>
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
    /// Collects every terminal word reachable from the provided node.
    /// </summary>
    /// <param name="node">Node from which traversal starts.</param>
    /// <param name="prefix">Current byte prefix built before reaching <paramref name="node"/>.</param>
    /// <param name="results">Collection that receives discovered words.</param>
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
    /// Finds all words that end with the specified suffix.
    /// </summary>
    /// <param name="prefix">Suffix to match.</param>
    /// <param name="maxLength">Maximum word length to return.</param>
    /// <returns>Words that end with the supplied suffix.</returns>
    public List<string> SearchBySuffix(string prefix, int maxLength = int.MaxValue)
    {
        return SearchByPattern("*" + prefix.ToUpper());
    }

    /// <summary>
    /// Finds all words that match a pattern containing optional wildcard characters.
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
    /// Recursively evaluates a byte pattern against the DAWG graph.
    /// </summary>
    /// <param name="currentNode">Current graph node.</param>
    /// <param name="bytePattern">Pattern encoded as tile bytes and wildcard bytes.</param>
    /// <param name="patternIndex">Current index in the pattern.</param>
    /// <param name="currentWord">Current candidate word bytes.</param>
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
    /// Recursively traverses the DAWG and collects every terminal word in the requested length range.
    /// </summary>
    /// <param name="currentNode">Current graph node.</param>
    /// <param name="word">Mutable buffer containing the current word bytes.</param>
    /// <param name="results">Collection that receives discovered words.</param>
    /// <param name="minLength">Minimum accepted word length.</param>
    /// <param name="maxLength">Maximum accepted word length.</param>
    /// <returns>Always returns <c>false</c>; traversal results are written to <paramref name="results"/>.</returns>
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
    /// Converts a search pattern into configured tile bytes and wildcard marker bytes.
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


