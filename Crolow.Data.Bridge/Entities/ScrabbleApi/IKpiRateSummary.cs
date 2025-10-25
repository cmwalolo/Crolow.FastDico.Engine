using Crolow.FastDico.Common.Models.ScrabbleApi.Kpi;

namespace Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi
{
    public interface IKpiRateSummary
    {
        SortedDictionary<KpiKeys, IKpiRateSummaryItem> Summary { get; set; }
        public void Sum(IKpiRateSummary other);
    }
}