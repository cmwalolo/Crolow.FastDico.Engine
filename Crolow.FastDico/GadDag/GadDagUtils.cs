using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.Utils;

namespace Crolow.FastDico.GadDag
{
    /// <summary>
    /// Provides utility operations for checking words and one-letter extensions in a GADDAG graph.
    /// </summary>
    public class GadDagUtils
    {
        /// <summary>
        /// Recursively follows a byte-encoded word through the graph.
        /// </summary>
        /// <param name="currentNode">Current graph node.</param>
        /// <param name="word">Word represented as tile bytes.</param>
        /// <param name="index">Current byte index.</param>
        /// <returns>The terminal node when the word is found; otherwise, <c>null</c>.</returns>
        private ILetterNode SearchWordRecursive(ILetterNode currentNode, List<byte> word, int index)
        {
            if (index == word.Count)
            {
                return currentNode.IsEnd ? currentNode : null;
            }

            byte currentByte = word[index];

            // Traverse children to find the matching letter
            foreach (var child in currentNode.Children)
            {
                if (child.Letter == currentByte) //|| child.Letter == tilesUtils.PivotByte) // Pivot handling
                {
                    var node = SearchWordRecursive(child, word, index + 1);
                    if (node != null)
                    {
                        return node;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Finds one-letter prefixes and suffixes that can extend an existing word.
        /// </summary>
        /// <param name="tilesUtils">Tile utility used to encode and decode letters.</param>
        /// <param name="rootNode">Root node of the GADDAG graph.</param>
        /// <param name="word">Word to extend.</param>
        /// <returns>Pairs where the key identifies the side and the value contains the extension letter.</returns>
        public List<KeyValuePair<int, string>> FindPlusOne(ITilesUtils tilesUtils, ILetterNode rootNode, string word)
        {
            var bytes = tilesUtils.ConvertWordToBytes(word);
            var node = SearchWordRecursive(rootNode, bytes, 0);
            var result = new List<KeyValuePair<int, string>>();

            // Find one letter after the word
            foreach (var child in node.Children.Where(p => p.IsEnd))
            {
                var letter = tilesUtils.ConvertBytesToWord((new[] { child.Letter }).ToList());
                result.Add(new KeyValuePair<int, string>(1, letter));
            }

            // Find one letter before the word
            node = node.Children.FirstOrDefault(p => p.IsPivot);
            if (node != null)
            {
                foreach (var child in node.Children.Where(p => p.IsEnd))
                {
                    var letter = tilesUtils.ConvertBytesToWord((new[] { child.Letter }).ToList());
                    result.Add(new KeyValuePair<int, string>(0, letter));
                }
            }


            return result;
        }

        /// <summary>
        /// Checks whether the supplied tile sequence exists as a word in the graph.
        /// </summary>
        /// <param name="rootNode">Root node of the GADDAG graph.</param>
        /// <param name="tiles">Tiles that form the candidate word.</param>
        /// <returns><c>true</c> when the tile sequence is present as a terminal word.</returns>
        public bool CheckWord(ILetterNode rootNode, List<Tile> tiles)
        {
            var bytes = tiles.Select(p => p.Letter).ToList();
            var node = SearchWordRecursive(rootNode, bytes, 0);
            return node != null;
        }

    }
}
