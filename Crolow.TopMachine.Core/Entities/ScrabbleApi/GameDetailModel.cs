using Crolow.FastDico.Common.Models.ScrabbleApi.Kpi;
using Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi;
using Crolow.TopMachine.Data.Bridge.DataObjects;
using Kalow.Apps.Common.DataTypes;

namespace Crolow.FastDico.Common.Models.ScrabbleApi.Game;

public class GameDetailModel : DataObject, IGameDetailModel
{
    public KalowId GameConfigurationId { get; set; } = KalowId.Empty;
    public List<KalowId> Users { get; set; } = new List<KalowId>();
    public List<IPlayableSolutionModel> Rounds { get; set; } = new List<IPlayableSolutionModel>();
    public int TotalPoints { get; set; }
    public float PlayTime { get; set; }
    public DateTime DateTime { get; set; } = DateTime.UtcNow;
    public string ConfigurationName { get; set; } = string.Empty;
    public KpiRateSummary KpiRateSummary { get; set; } = new KpiRateSummary();

    public GameDetailModel()
    {
    }
}
