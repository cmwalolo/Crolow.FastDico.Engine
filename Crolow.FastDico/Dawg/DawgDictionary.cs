using Crolow.FastDico.Base;
using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Dicos;
using Crolow.FastDico.Utils;

namespace Crolow.FastDico.Dawg;

public class DawgDictionary : BaseDictionary
{
    public DawgDictionary(ITilesUtils tilesUtils) : base(tilesUtils)
    {
    }

    public override void Insert(string word)
    {
        var chars = tilesUtils.ConvertWordToBytes(word.ToUpper());
        var currentNode = RootBuild;

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
