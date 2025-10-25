using Crolow.TopMachine.Data.Bridge.DataObjects;
using Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi;
using Kalow.Apps.Common.DataTypes;

namespace Crolow.FastDico.Common.Models.ScrabbleApi.Entities
{


    public class GameHypergramConfigModel : DataObject, IGameHypergramConfigModel
    {
        public GameHypergramConfigModel()
        {
        }
        public string Name { get; set; }
        public KalowId LetterConfig { get; set; }
        public KalowId Dictionary { get; set; }
        public KalowId ReferenceDictionary { get; set; }
        public int InRackLetters { get; set; }
        public int TimeByTurn { get; set; }
        public bool JokerMode { get; set; }
        public string LettersOnRack { get; set; }
    }
}
