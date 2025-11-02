using Crolow.FastDico.Data.Bridge.Entities.Puzzling;
using Crolow.TopMachine.Data.Bridge.DataObjects;
using Kalow.Apps.Common.DataTypes;

namespace Crolow.TopMachine.Core.Entities.Puzzling
{

    public class GameRollerStatisticsModel : IGameRollerStatisticsModel
    {
        public string Date { get; set; }
        public float TotalTime { get; set; }
        public float BestTime { get; set; }
        public int AmountOfRounds { get; set; }
        public int MissedRounds { get; set; }
        public int OkRounds { get; set; }
    }

    public class GameRollerConfigModel : DataObject, IGameRollerConfigModel
    {

        public GameRollerConfigModel()
        {
        }
        public string Name { get; set; }
        public KalowId GameConfigId { get; set; }
        public string GameConfigName { get; set; }
        public int NumberOfRounds { get; set; }
        public string MandatoryLettersOnRack { get; set; }
        public string LettersOnRack { get; set; }
        public List<IGameRollerStatisticsModel> Statistics { get; set; }

    }
}
