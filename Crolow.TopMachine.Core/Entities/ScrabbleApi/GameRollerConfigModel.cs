using Crolow.TopMachine.Data.Bridge.DataObjects;
using Kalow.Apps.Common.DataTypes;

namespace Crolow.FastDico.Common.Models.ScrabbleApi.Entities
{
    public class GameRollerConfigModel : DataObject, IGameRollerConfigModel
    {
        public GameRollerConfigModel()
        {
        }
        public string Name { get; set; }
        public KalowId GameConfigId { get; set; }
        public int NumberOfRounds { get; set; }
        public string MandatoryLettersOnRack { get; set; }
        public string LettersOnRack { get; set; }

    }
}
