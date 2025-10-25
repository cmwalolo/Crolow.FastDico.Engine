using Crolow.TopMachine.Data.Bridge;
using Kalow.Apps.Common.DataTypes;

namespace Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi
{
    public interface IGameUserDetailModel : IDataObject
    {
        KalowId GameId { get; set; }
        KalowId GameConfigurationId { get; set; }
        float PlayTime { get; set; }
        List<IPlayableSolutionModel> Rounds { get; set; }
        int GamePoints { get; set; }
        int TotalPoints { get; set; }
        int MissedRounds { get; set; }

        KalowId User { get; set; }
        DateTime DateTime { get; set; }
        string UserName { get; set; }

        public float RateTime { get; set; }
        public float RateRound { get; set; }
        public float RateAll { get; set; }
        public IKpiRateSummary KpiRateSummary { get; set; }
    }

    public interface IGameDetailModel : IDataObject
    {
        KalowId GameConfigurationId { get; set; }

        string ConfigurationName { get; set; }
        List<KalowId> Users { get; set; }
        List<IPlayableSolutionModel> Rounds { get; set; }
        int TotalPoints { get; set; }
        float PlayTime { get; set; }
        DateTime DateTime { get; set; }


    }

    public interface IGameUserStatisticsModel : IDataObject
    {
        public KalowId UserId { get; set; }
        public KalowId ConfigurationId { get; set; }
        public string Date { get; set; }
        public int AmountOfGames { get; set; }
        public int MaxPoints { get; set; }
        public int TotalPoints { get; set; }
        public float TotalTime { get; set; }

        public float BestTime { get; set; }
        public float BestPercentage { get; set; }
        public int AmountOfTops { get; set; }
        public int MissedRounds { get; set; }
        public IKpiRateSummary KpiSummary { get; set; }

    }
}