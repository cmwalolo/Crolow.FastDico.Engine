using Crolow.FastDico.Base;
using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Dicos;
using Crolow.FastDico.Utils;

namespace Crolow.FastDico.GadDag
{
    /// <summary>
    /// Builds a GADDAG-style dictionary by inserting words as byte sequences both in left-to-right order and as
    /// reversed right-to-left combinations with a pivot byte.  
    /// </summary>
    /// <remarks>Converts input words to uppercase byte sequences via ITilesUtils. Inserts the full word, then
    /// for each split position constructs a sequence of the suffix, the pivot byte (TilesUtils.PivotByte), and the
    /// reversed prefix, inserting each sequence into the underlying trie rooted at RootBuild and marking terminal nodes
    /// with SetEnd.</remarks>
    public class GadDagDictionary : BaseDictionary
    {
        public GadDagDictionary(ITilesUtils tilesUtils) : base(tilesUtils)
        {
        }

        /// <summary>
        /// Inserts the given word (converted to uppercase and encoded to bytes) and its rotated variations with a pivot
        /// byte.   
        /// </summary>
        /// <remarks>Converts the word to bytes via tilesUtils.ConvertWordToBytes, inserts the full word
        /// in left-to-right order, then for each rotation inserts a sequence consisting of the suffix, the pivot byte
        /// ('#', TilesUtils.PivotByte), and the reversed prefix.</remarks>
        /// <param name="word">The word to insert.</param>
        public override void Insert(string word)
        {
            // Convert word to bytes
            List<byte> byteWord = tilesUtils.ConvertWordToBytes(word.ToUpper());

            // Add the full word in left-to-right order
            Insert(byteWord);

            // Add reversed-right-to-left combinations with '#' pivot
            int len = byteWord.Count;
            for (int i = 1; i < len; i++) // Skip the first position
            {
                List<byte> left = byteWord.GetRange(i, len - i);
                List<byte> right = byteWord.GetRange(0, i);

                right.Reverse();
                left.Add(TilesUtils.PivotByte); // Add the pivot '#'
                left.AddRange(right);
                Insert(left);
            }
        }

        /// <summary>
        /// Inserts the specified sequence of bytes into the trie, creating child nodes as needed and marking the final
        /// node as terminal.   
        /// </summary>
        /// <remarks>Existing prefixes are reused; duplicate insertions mark the same terminal node. Runs
        /// in O(n) time where n is the length of chars.</remarks>
        /// <param name="chars">The sequence of bytes to insert into the trie.</param>
        private void Insert(List<byte> chars)
        {
            var currentNode = RootBuild;
            var hasTerminated = false;
            foreach (var letter in chars)
            {
                ILetterNode childNode = currentNode.Children.FirstOrDefault(c => c.Letter == letter);

                if (childNode == null)
                {
                    childNode = new LetterNode { Letter = letter };
                    currentNode.Children.Add(childNode);
                }
                currentNode = childNode;
            }
            currentNode.SetEnd();
        }
    }
}
