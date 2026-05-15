using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Utils;

namespace Crolow.FastDico.Dicos;

public class LetterNode : ILetterNode
{
    /// <summary>
    /// In Gaddag the Letter 31 represents a pivot node.
    /// </summary>
    /// <remarks>
    /// Control and Letter could be merged together into a single byte with bitwise operations
    /// WE just need to change the IsEnd Property and SetEnd Method and use a different value for TilesUtiles.IsEnd
    /// </remarks>
    public byte Control { get; set; }

    /// <summary>
    /// Gets or sets the byte value that represents a letter.   
    /// </summary>
    /// <remarks>Interpretation depends on the character encoding in use; convert to a char or string using
    /// the appropriate encoding when needed.</remarks>
    public byte Letter { get; set; }

    /// <summary>
    /// True when the Control bitmask includes the TilesUtils.IsEnd flag.   
    /// </summary>
    /// <remarks>Computed from the Control bitmask by performing a bitwise AND with
    /// TilesUtils.IsEnd.</remarks>
    public bool IsEnd { get { return (Control & TilesUtils.IsEnd) == TilesUtils.IsEnd; } }

    /// <summary>
    /// Indicates whether the Letter equals the pivot marker defined by TilesUtils.PivotByte.
    /// </summary>
    /// <remarks>Read-only computed property. The result reflects the current value of TilesUtils.PivotByte at
    /// access time.</remarks>
    public bool IsPivot { get { return Letter == TilesUtils.PivotByte; } }

    /// <summary>
    /// Sets the end flag on the node.
    /// </summary>  
    public void SetEnd() { Control |= TilesUtils.IsEnd; }

    /// <summary>
    /// Gets or sets the list of child nodes.
    /// </summary>
    public List<ILetterNode> Children { get; set; } = new List<ILetterNode>();
}
