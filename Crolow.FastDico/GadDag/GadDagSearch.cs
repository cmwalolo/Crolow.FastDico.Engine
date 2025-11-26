using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Utils;

namespace Crolow.FastDico.GadDag;

public class GadDagSearch : IDawgSearch
{
    public ILetterNode Root { get; private set; }
    public ITilesUtils tilesUtils { get; private set; }

    public GadDagSearch(ILetterNode root, ITilesUtils tilesUtils)
    {
        Root = root;
        this.tilesUtils = tilesUtils;
    }

    public List<string> SearchAllWords(int minLength, int maxLength)
    {
        var results = new List<string>();
        SearchAllWordsRecursive(Root, new List<byte>(), results, minLength, maxLength);
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
        foreach (var child in currentNode.Children.Where(p => p.Letter != TilesUtils.PivotByte))
        {
            word.Add(child.Letter);
            SearchAllWordsRecursive(child, word, results, minLength, maxLength);
            word.RemoveAt(word.Count - 1); // Backtrack     
        }

        return false;
    }

    public bool SearchWord(string word)
    {
        var bytes = tilesUtils.ConvertWordToBytes(word.ToUpper());
        return SearchWordRecursive(Root, bytes, 0);
    }

    public bool SearchWord(List<byte> word)
    {
        return SearchWordRecursive(Root, word, 0);
    }

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

    public List<string> SearchByPattern(string pattern, int minLength = int.MinValue, int maxLength = int.MaxValue)
    {
        // Convert the pattern into bytes
        List<byte> bytePattern = ConvertPatternToBytes(pattern.ToUpper());
        List<string> results = new List<string>();
        SearchByPatternRecursive(Root, bytePattern, 0, new List<byte>(), results, minLength, maxLength);
        return results;
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
