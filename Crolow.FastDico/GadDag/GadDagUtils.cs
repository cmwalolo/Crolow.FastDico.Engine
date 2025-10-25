using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.Utils;

namespace Crolow.FastDico.GadDag
{
    public class GadDagUtils
    {
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

        public bool CheckWord(ILetterNode rootNode, List<Tile> tiles)
        {
            var bytes = tiles.Select(p => p.Letter).ToList();
            var node = SearchWordRecursive(rootNode, bytes, 0);
            return node != null;
        }

    }
}
