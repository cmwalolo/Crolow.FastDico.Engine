using Kalow.Apps.Common.DataTypes;

namespace Crolow.TopMachine.Data.Bridge.Entities.Dictionaries
{
    public interface IDictionaryModel : IDataObject
    {
        public KalowId MainDictionaryId { get; set; }
        KalowId LetterConfig { get; set; }
        string DictionaryFile { get; set; }
        bool IsDefault { get; set; }
        string Language { get; set; }
        string Name { get; set; }
    }
}