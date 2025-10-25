using Crolow.FastDico.Base;
using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Dicos;
using Crolow.FastDico.Utils;

namespace Crolow.FastDico.GadDag
{
    public class GadDagDictionary : BaseDictionary
    {
        public GadDagDictionary(ITilesUtils tilesUtils) : base(tilesUtils)
        {
        }

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
