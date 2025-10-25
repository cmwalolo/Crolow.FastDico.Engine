using Crolow.TopMachine.Data.Bridge.DataObjects;
using Crolow.TopMachine.Data.Bridge.Entities.Dictionaries;
using Kalow.Apps.Common.DataTypes;

namespace Crolow.TopMachine.Core.Entities.Definitions;
public class DictionaryModel : DataObject, IDictionaryModel
{
    public KalowId LetterConfig { get; set; }
    public KalowId MainDictionaryId { get; set; }
    public bool IsDefault { get; set; }
    public string Name { get; set; }
    public string Language { get; set; }
    public string DictionaryFile { get; set; }
    
}
