using Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi;

namespace Crolow.FastDico.Common.Models.ScrabbleApi.Kpi
{
    public readonly record struct KpiRateSummaryItem(int Count, int Success) : IKpiRateSummaryItem
    {
        public KpiRateSummaryItem Increment(bool success)
            => new(Count + 1, Success + (success ? 1 : 0));

        public static KpiRateSummaryItem Sum(IKpiRateSummaryItem a, IKpiRateSummaryItem b)
            => new(a.Count + b.Count, a.Success + b.Success);
    }
}
