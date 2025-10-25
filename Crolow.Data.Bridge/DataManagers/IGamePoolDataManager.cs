using Crolow.TopMachine.Data.Bridge;
using Kalow.Apps.Common.DataTypes;

namespace Crolow.TopMachine.Data.Repositories
{
    public interface IGamePoolDataManager<T> : IDataManager<T> where T : IDataObject
    {
        Task<int> CountAvailablePoolAsync(KalowId configId);
        Task<T> GetGameAsync(KalowId configId, KalowId userId);
        Task<Dictionary<KalowId, int>> CountConfigs(KalowId userId);

    }
}