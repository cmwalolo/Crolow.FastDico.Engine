using Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi;
using Crolow.TopMachine.Data.Bridge.DataObjects;
using Kalow.Apps.Common.DataTypes;
using Kalow.Apps.Common.JsonConverters;
using Newtonsoft.Json;

namespace Crolow.FastDico.Common.Models.ScrabbleApi.Game;

public class GameUserStatisticsModel : DataObject, IGameUserStatisticsModel
{
    [JsonConverter(typeof(KalowIdConverter))]

    public KalowId UserId { get; set; }

    [JsonConverter(typeof(KalowIdConverter))]

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
