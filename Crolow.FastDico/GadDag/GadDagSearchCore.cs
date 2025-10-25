using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Search;
using Crolow.FastDico.Utils;

namespace Crolow.FastDico.GadDag;
public class GadDagSearchCore
{
    public int MaxResults { get; private set; }
    private ILetterNode Root;
    public WordTilesUtils wordTilesUtils;

    public GadDagSearchCore(IDictionaryContainer container, int maxResults)
    {
        Root = container.Dico.Root;
        MaxResults = maxResults;
        wordTilesUtils = new WordTilesUtils(container.TilesUtils);
    }

    // Anagramic searches From Letters + 1, Greater, smaller
    public WordResults FindAllWordsFromLetters(string pattern, string optional)
    {
        var letters = wordTilesUtils.ConvertWordToBytes(pattern.ToUpper(), optional.ToLower());
        var results = new WordResults();


        FindWordsUsingLetters(Root, letters, new WordResults.Word(), results, true);
        return results;
    }

    public WordResults FindAllWordsSmaller(string pattern, int minLength = 0)
    {
        var letters = wordTilesUtils.ConvertWordToBytes(pattern.ToUpper(), "");
        var results = new WordResults();

        FindWordsUsingLetters(Root, letters, new WordResults.Word(), results, false);
        results.Words = results.Words.Where(p => p.Tiles.Count >= minLength && p.Tiles.Count != pattern.Length).ToList();

        return results;
    }

    public WordResults FindAllWordsGreater(string pattern, int maxLength)
    {
        var results = new WordResults();
        var optional = "?";

        for (int x = 0; x < maxLength; x++)
        {
            var letters = wordTilesUtils.ConvertWordToBytes(pattern.ToUpper(), optional);
            FindWordsUsingLetters(Root, letters, new WordResults.Word(), results, true);
            optional += "?";
        }
        return results;
    }

    public WordResults FindAllWordsContaining(string pattern)
    {
        string search = $"*{pattern.ToUpper()}*";
        var letters = wordTilesUtils.ConvertWordToBytes(search, "");
        var results = new WordResults();
        var optional = "?";
        SearchByPatternRecursive(Root, letters, 0, new WordResults.Word(), results);
        return results;
    }

    public WordResults FindAllWordsWithPattern(string pattern)
    {
        var letters = wordTilesUtils.ConvertWordToBytes(pattern.ToUpper(), "");
        var results = new WordResults();
        var optional = "?";
        SearchByPatternRecursive(Root, letters, 0, new WordResults.Word(), results);
        return results;
    }

    public WordResults FindAllWordsMoreOrLess(string searchPattern)
    {
        var results = new WordResults();
        var pattern = searchPattern.ToUpper();
        results.Words.AddRange(FindAllWordsFromLetters(pattern, "").Words);


        for (int x = 0; x < pattern.Length; x++)
        {
            var pat = pattern.ToArray();
            pat[x] = '?';
            results.Words.AddRange(FindAllWordsFromLetters(new string(pat), "").Words);
        }

        for (int x = 0; x <= pattern.Length; x++)
        {
            var pat = pattern.ToList();

            if (x < pattern.Length)
            {
                pat.Insert(x, '?');
            }
            else
            {
                pat.Add('?');
            }

            results.Words.AddRange(FindAllWordsFromLetters(new string(pat.ToArray()), "").Words);
        }
        var finalResults = new WordResults();

        var group = results.Words
            .Select(p => new { w = wordTilesUtils.ConvertBytesToWordForDisplay(p).ToUpper(), i = p })
            .GroupBy(p => p.w);

        foreach (var item in group)
        {
            var w = item.First().w.ToUpper();
            var r = WordResultEvaluator.CompareWords(pattern, w);

            if (r.HasAnySet())
            {
                for (int x = 1; x <= 5; x++)
                {
                    if (r.Get(x))
                    {
                        finalResults.Words.Add(new WordResults.Word(item.First().i.Tiles, x));
                    }
                }
            }
        }
        finalResults.Words = finalResults.Words.Where(p => p.Status > 0).ToList();
        return finalResults;
    }

    #region Pattern search 
    private void SearchByPatternRecursive(ILetterNode currentNode, List<WordResults.ResultTile> bytePattern, int patternIndex, WordResults.Word currentWord, WordResults results)
    {
        // Base case: Reached the end of the pattern
        if (patternIndex == bytePattern.Count)
        {
            if (currentNode.IsEnd)
            {
                results.Words.Add(new WordResults.Word(currentWord));
            }
            return;
        }

        WordResults.ResultTile currentByte = bytePattern[patternIndex];

        if (currentByte.Letter == TilesUtils.WildcardByte) // '*' wildcard
        {
            // Match zero or more characters
            // First, try skipping the '*'
            SearchByPatternRecursive(currentNode, bytePattern, patternIndex + 1, currentWord, results);

            // Then, try matching one or more characters
            foreach (var child in currentNode.Children)
            {
                if (child.Letter != TilesUtils.PivotByte)
                {
                    currentWord.Tiles.Add(new WordResults.ResultTile(child.Letter, false, 1));
                    SearchByPatternRecursive(child, bytePattern, patternIndex, currentWord, results);
                    currentWord.Tiles.RemoveAt(currentWord.Tiles.Count - 1); // Backtrack
                }
            }
        }
        else if (currentByte.Letter == TilesUtils.JokerByte) // '?' wildcard
        {
            // Match exactly one character
            foreach (var child in currentNode.Children)
            {
                if (child.Letter != TilesUtils.PivotByte)
                {
                    currentWord.Tiles.Add(new WordResults.ResultTile(child.Letter, true, 1));
                    SearchByPatternRecursive(child, bytePattern, patternIndex + 1, currentWord, results);
                    currentWord.Tiles.RemoveAt(currentWord.Tiles.Count - 1); // Backtrack
                }
            }
        }
        else
        {
            // Match the exact character
            var nextNode = currentNode.Children.Where(p => p.Letter != TilesUtils.PivotByte && p.Letter == currentByte.Letter);
            if (nextNode.Any())
            {
                currentWord.Tiles.Add(new WordResults.ResultTile(currentByte.Letter, true, 0));
                SearchByPatternRecursive(nextNode.First(), bytePattern, patternIndex + 1, currentWord, results);
                currentWord.Tiles.RemoveAt(currentWord.Tiles.Count - 1); // Backtrack
            }
        }
    }
    #endregion

    #region Anagram search
    // Recursive helper for both functions
    private void FindWordsUsingLetters(
        ILetterNode currentNode,
        List<WordResults.ResultTile> availableLetters,
        WordResults.Word currentWord,
        WordResults results,
        bool requireExactMatch)
    {
        if (results.Words.Count == MaxResults)
        {
            return;
        }
        foreach (var child in currentNode.Children)
        {
            if (child.Letter != TilesUtils.PivotByte)
            {
                if (!requireExactMatch || (availableLetters.Any(p => p.Letter == child.Letter) || availableLetters.Any(p => p.IsJoker)))
                {
                    bool isJoker = false;

                    var l = availableLetters.FirstOrDefault(p => p.Letter == child.Letter);
                    if (l != null)
                    {
                        currentWord.Tiles.Add(l);
                        availableLetters.Remove(l);
                    }
                    else
                    {
                        l = availableLetters.FirstOrDefault(p => p.IsJoker);
                        if (l != null)
                        {
                            isJoker = true;
                            currentWord.Tiles.Add(new WordResults.ResultTile(child.Letter, true, l.Status));
                            availableLetters.Remove(l);
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (child.IsEnd)
                    {
                        if (!requireExactMatch || availableLetters.Count == 0)
                        {
                            results.Words.Add(new WordResults.Word(currentWord));
                        }
                    }

                    FindWordsUsingLetters(child, availableLetters, currentWord, results, requireExactMatch);

                    // Backtrack to restore statev
                    if (!isJoker)
                    {
                        availableLetters.Add(l);
                    }
                    else
                    {
                        availableLetters.Add(l);
                    }

                    currentWord.Tiles.RemoveAt(currentWord.Tiles.Count - 1);
                    isJoker = false;
                }
            }
        }
    }
    #endregion
}
