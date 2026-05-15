using Crolow.FastDico.Base;
using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Dicos;
using Crolow.FastDico.Utils;

namespace Crolow.FastDico.Dawg;

/// <summary>
/// Represents a dictionary backed by a directed acyclic word graph (DAWG) that stores words using shared-prefix letter
/// nodes for compact representation.   
/// </summary>
/// <remarks>Words are normalized to upper-case and converted to byte sequences via the provided ITilesUtils
/// before insertion. Insertion builds letter nodes under the build root and marks terminal nodes. Inherits root and
/// common behavior from BaseDictionary. Instances are not guaranteed to be thread-safe for concurrent
/// modifications.</remarks>
public class DawgDictionary : BaseDictionary
{
    public DawgDictionary(ITilesUtils tilesUtils) : base(tilesUtils)
    {
    }

    /// <summary>
    /// Inserts a word into the trie by converting it to tile bytes, creating missing letter nodes as needed, and
    /// marking the final node as a terminal node.  
    /// </summary>
    /// <remarks>Uses tilesUtils.ConvertWordToBytes to derive the letter sequence. Existing child nodes are
    /// reused; new LetterNode instances are added for missing letters. Calls SetEnd() on the terminal node to mark the
    /// word boundary.</remarks>
    /// <param name="word">Word to insert; normalized to uppercase and converted to tile bytes before traversal and insertion.</param>
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
