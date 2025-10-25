using Crolow.FastDico.Common.Models.ScrabbleApi.Kpi;
using Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi;
using Crolow.TopMachine.Data.Bridge.DataObjects;
using Kalow.Apps.Common.DataTypes;

namespace Crolow.FastDico.Common.Models.ScrabbleApi.Game;

public class GameUserDetailModel : DataObject, IGameUserDetailModel
{
    public KalowId GameId { get; set; } = KalowId.Empty;
    public KalowId GameConfigurationId { get; set; } = KalowId.Empty;
    public KalowId User { get; set; } = KalowId.Empty;
    public List<IPlayableSolutionModel> Rounds { get; set; } = new List<IPlayableSolutionModel>();
    public int GamePoints { get; set; }
    public int TotalPoints { get; set; }
    public float PlayTime { get; set; }
    public DateTime DateTime { get; set; } = DateTime.UtcNow;
    public string UserName { get; set; } = string.Empty;

    public float RateTime { get; set; }
    public float RateRound { get; set; }
    public float RateAll { get; set; }
    public int MissedRounds { get; set; }

    public IKpiRateSummary KpiRateSummary { get; set; } = new KpiRateSummary();

    public GameUserDetailModel()
    {
    }
}
