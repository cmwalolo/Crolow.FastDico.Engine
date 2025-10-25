using System.Collections;

namespace Crolow.FastDico.GadDag
{
    public static class WordResultEvaluator
    {
        //static void Main()
        //{
        //    string baseWord = "loure";
        //    List<string> words = new List<string> { "ourle", "louer", "roule", "louer", "boure", "coure", "toure", "laure", "lougre", "lourde", "loue" };

        //    foreach (var word in words)
        //    {
        //        List<string> results = CompareWords(baseWord, word);
        //        BufferedConsole.WriteLine($"{word}: {string.Join(", ", results)}");
        //    }
        //}

        public static BitArray CompareWords(string baseWord, string word)
        {
            BitArray bitSet = new BitArray(6);
            if (CanMoveOneLetter(baseWord, word)) bitSet.Set(1, true);
            if (CanInvertTwoLetters(baseWord, word)) bitSet.Set(2, true);
            if (CanReplaceOneLetter(baseWord, word)) bitSet.Set(3, true);
            if (CanInsertOneLetter(baseWord, word)) bitSet.Set(4, true);
            if (CanRemoveOneLetter(baseWord, word)) bitSet.Set(5, true);
            return bitSet;
        }

        static bool CanMoveOneLetter(string baseWord, string word)
        {
            if (baseWord.Length != word.Length) return false;

            for (int i = 0; i < baseWord.Length; i++)
            {
                for (int j = 0; j < baseWord.Length; j++)
                {
                    if (i == j) continue; // Skip same position

                    // Create a new version of baseWord with the letter moved
                    char[] temp = baseWord.ToCharArray();
                    char letter = temp[i];

                    // Remove the letter and reinsert it at a different position
                    List<char> modified = new List<char>(temp);
                    modified.RemoveAt(i);
                    modified.Insert(j, letter);

                    if (new string(modified.ToArray()) == word)
                        return true;
                }
            }
            return false;
        }

        static bool CanInvertTwoLetters(string baseWord, string word)
        {
            if (baseWord.Length != word.Length) return false;

            int diffCount = 0;
            int[] diffIndexes = new int[2];

            for (int i = 0; i < baseWord.Length; i++)
            {
                if (baseWord[i] != word[i])
                {
                    if (diffCount < 2) diffIndexes[diffCount] = i;
                    diffCount++;
                }
            }

            return diffCount == 2 && baseWord[diffIndexes[0]] == word[diffIndexes[1]] && baseWord[diffIndexes[1]] == word[diffIndexes[0]];
        }

        static bool CanReplaceOneLetter(string baseWord, string word)
        {
            return baseWord.Length == word.Length && baseWord.Zip(word, (a, b) => a != b).Count(x => x) == 1;
        }

        static bool CanInsertOneLetter(string baseWord, string word)
        {
            if (word.Length != baseWord.Length + 1) return false;

            int i = 0, j = 0;
            bool oneInsertion = false;

            while (i < baseWord.Length && j < word.Length)
            {
                if (baseWord[i] == word[j])
                {
                    i++;
                    j++;
                }
                else if (!oneInsertion) // Allow inserting exactly one character
                {
                    oneInsertion = true;
                    j++; // Move only in `word` to simulate insertion
                }
                else
                {
                    return false; // More than one mismatch, not a single insertion
                }
            }
            return true; // Ensures only one letter was inserted
        }


        static bool CanRemoveOneLetter(string baseWord, string word)
        {
            return CanInsertOneLetter(word, baseWord);
        }
    }
}
