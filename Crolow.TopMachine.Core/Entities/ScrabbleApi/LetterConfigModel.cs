using Crolow.TopMachine.Data.Bridge.DataObjects;
using Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi;
using Kalow.Apps.Common.DataTypes;

namespace Crolow.FastDico.Common.Models.ScrabbleApi.Entities
{
    public class LetterConfigModel : DataObject, ILetterConfigModel
    {
        public string Name { get; set; }
        public List<ITileConfig> Letters { get; set; }

        public KalowId DictionaryId { get; set; }

        public LetterConfigModel()
        {
            Id = KalowId.NewObjectId();
        }
    }
}
