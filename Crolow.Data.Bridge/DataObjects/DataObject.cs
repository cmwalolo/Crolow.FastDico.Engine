using Crolow.TopMachine.Data.Bridge;
using Kalow.Apps.Common.DataTypes;
using LiteDB;
using Newtonsoft.Json;

namespace Crolow.TopMachine.Data.Bridge.DataObjects
{
    public class DataObject : IDataObject
    {
        public DataObject()
        {
            Id = KalowId.Empty;
            EditState = EditState.Unchanged;
        }
        [JsonProperty("_id")]
        public KalowId Id { get; set; }


        [BsonIgnore]
        public EditState EditState { get; set; }
    }
}