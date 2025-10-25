using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Utils;

namespace Crolow.FastDico.Dicos;

public class LetterNode : ILetterNode
{
    /// <summary>
    /// In Gaddag the Letter 31 represents a pivot node.
    /// </summary>
    public byte Control { get; set; }
    public byte Letter { get; set; }
    public bool IsEnd { get { return (Control & TilesUtils.IsEnd) == TilesUtils.IsEnd; } }
    public bool IsPivot { get { return Letter == TilesUtils.PivotByte; } }

    public void SetEnd() { Control |= TilesUtils.IsEnd; }

    public List<ILetterNode> Children { get; set; } = new List<ILetterNode>();
}
