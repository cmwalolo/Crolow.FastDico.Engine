using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Utils;

namespace Crolow.FastDico.Dawg;

public class DawgSearch : IDawgSearch
{
    public ILetterNode Root { get; private set; }

    protected ITilesUtils tilesUtils;
    public DawgSearch(ILetterNode root, ITilesUtils tilesUtils)
    {
        Root = root;
        this.tilesUtils = tilesUtils;
    }

    #region Search functions


    // Search for a specific word
    public List<string> SearchAllWords(int minLength, int maxLength)
    {
        var results = new List<string>();
        SearchAllWordsRecursive(Root, new List<byte>(), results, minLength, maxLength);
        return results;
    }
    public bool SearchWord(string word)
    {
        List<byte> byteWord = tilesUtils.ConvertWordToBytes(word.ToUpper());
        return SearchWord(byteWord);
    }

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

    public List<string> SearchBySuffix(string prefix, int maxLength = int.MaxValue)
    {
        return SearchByPattern("*" + prefix.ToUpper());
    }

    // Helper method to find all words from a given node
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
    public List<string> SearchByPattern(string pattern, int minLength = int.MinValue, int maxLength = int.MaxValue)
    {
        // Convert the pattern into bytes
        List<byte> bytePattern = ConvertPatternToBytes(pattern.ToUpper());
        List<string> results = new List<string>();
        SearchByPatternRecursive(Root, bytePattern, 0, new List<byte>(), results, minLength, maxLength);
        return results;
    }

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
        else if (currentByte == TilesUtils.SingleMatchByte) // '?' wildcard
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

    private List<byte> ConvertPatternToBytes(string pattern)
    {
        List<byte> bytePattern = new List<byte>();
        foreach (char c in pattern)
        {
            if (c == '*')
                bytePattern.Add(TilesUtils.WildcardByte); // '*' wildcard
            else if (c == '?')
                bytePattern.Add(TilesUtils.SingleMatchByte); // '?' wildcard
            else
                bytePattern.Add(tilesUtils.configuration.LettersByChar[c].Letter);
        }
        return bytePattern;
    }

    // Function 1: Find all words that can be formed using exactly the given letters
    public List<string> FindAllWordsFromLetters(string pattern)
    {
        var letters = tilesUtils.ConvertWordToBytes(pattern.ToUpper());

        var results = new List<string>();
        FindWordsUsingLetters(Root, letters, new List<byte>(), new List<byte>(), results, true);
        return results;
    }

    // Function 2: Find all words that contain at least one of the given letters
    public List<string> FindAllWordsContainingLetters(string pattern)
    {
        var letters = tilesUtils.ConvertWordToBytes(pattern.ToUpper());
        var results = new List<string>();
        FindWordsUsingLetters(Root, letters, new List<byte>(), new List<byte>(), results, false);
        return results;
    }

    // Recursive helper for both functions
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


