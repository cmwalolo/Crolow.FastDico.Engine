using Crolow.TopMachine.Data.Bridge;
using Kalow.Apps.Common.DataTypes;

namespace Crolow.TopMachine.Data.Repositories
{
    public interface IGameUsersDataManager<T> : IDataManager<T> where T : IDataObject
    {
        Task<List<T>> GetGameDetailsAsync(KalowId gameId);
        Task<Tuple<int, List<T>>> GetUserHistory(KalowId userId, KalowId configId, int pageSize = 20, int page = 0);

    }

    public interface IGameUserStatisticsDataManager<T> : IDataManager<T> where T : IDataObject
    {
        Task<List<T>> GetUserStatsByDate(KalowId userId, string date, KalowId configId);
    }

}