using Kalow.Apps.Common.DataTypes;

namespace Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi
{
    public interface ILetterConfigModel : IDataObject
    {
        KalowId DictionaryId { get; set; }
        List<ITileConfig> Letters { get; set; }
        string Name { get; set; }
    }
}