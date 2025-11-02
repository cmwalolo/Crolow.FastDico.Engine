using Crolow.TopMachine.Data.Bridge;
using Kalow.Apps.Common.DataTypes;

namespace Crolow.FastDico.Data.Bridge.Entities.Puzzling
{
    public interface IGameRollerConfigModel : IDataObject
    {
        public KalowId GameConfigId { get; set; }
        public string GameConfigName { get; set; }
        public string LettersOnRack { get; set; }
        public string MandatoryLettersOnRack { get; set; }
        public string Name { get; set; }
        public int NumberOfRounds { get; set; }
        public List<IGameRollerStatisticsModel> Statistics { get; set; }
    }

    public interface IGameRollerStatisticsModel
    {
        public string Date { get; set; }
        public float TotalTime { get; set; }
        public float BestTime { get; set; }
        public int OkRounds { get; set; }
        public int AmountOfRounds { get; set; }
        public int MissedRounds { get; set; }
    }
}