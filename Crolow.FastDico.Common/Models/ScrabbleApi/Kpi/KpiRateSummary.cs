using Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi;

namespace Crolow.FastDico.Common.Models.ScrabbleApi.Kpi
{
    public class KpiRateSummary : IKpiRateSummary
    {
        public SortedDictionary<KpiKeys, IKpiRateSummaryItem> Summary { get; set; }
            = new SortedDictionary<KpiKeys, IKpiRateSummaryItem>();

        public KpiRateSummary()
        {
        }
        public void Calculate(List<KpiRate> rates, List<bool> found)
        {
            Summary.Clear();

            for (int i = 0; i < rates.Count; i++)
            {
                var rate = rates[i];
                if (i < found.Count)
                {
                    bool isSuccess = found[i];

                    for (int key = 0; key < 128; key++)
                    {
                        if (!rate.Get(key)) continue;

                        if (Summary.TryGetValue((KpiKeys)key, out var data))
                            Summary[(KpiKeys)key] = ((KpiRateSummaryItem)data).Increment(isSuccess);
                        else
                            Summary[(KpiKeys)key] = new KpiRateSummaryItem(1, isSuccess ? 1 : 0);
                    }
                }
            }
        }

        public void Sum(IKpiRateSummary other)
        {
            foreach (var kvp in other.Summary)
            {
                if (Summary.TryGetValue(kvp.Key, out var existing))
                    Summary[kvp.Key] = KpiRateSummaryItem.Sum(existing, kvp.Value);
                else
                    Summary[kvp.Key] = kvp.Value;
            }
        }
    }
}
