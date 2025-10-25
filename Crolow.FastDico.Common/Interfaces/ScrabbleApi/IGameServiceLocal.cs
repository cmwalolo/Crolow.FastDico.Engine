using Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi;
using Kalow.Apps.Common.DataTypes;

namespace Crolow.FastDico.Common.Interfaces.ScrabbleApi
{
    public interface IGameServiceLocal : IGameServiceBase
    {

    }

    public interface IGameServiceRemote : IGameServiceBase
    {

    }

    public interface IGameServiceBase
    {
        Task<bool> PurgeData(int daysToKeep);
        Task<IGameDetailModel> GetGameAsync(KalowId gameId);
        Task<bool> UpdateGameAsync(IGameDetailModel detailModel);

        Task<bool> UpdateUserGameAsync(IGameDetailModel game, IGameUserDetailModel detailModel);
        Task<int> CountPoolAsync(KalowId configId);
        Task<Dictionary<KalowId, int>> CountAllPoolsAsync(KalowId userId);

        Task<IGameDetailModel> GetGameAsync(KalowId configId, KalowId userId);
        Task<List<IGameUserDetailModel>> GetGameDetailsAsync(KalowId gameId);
        Task<List<IGameUserStatisticsModel>> GetUserStatisticsAsync(KalowId userId, KalowId configId, DateTime dtm);
        Task<Tuple<int, List<IGameUserDetailModel>>> GetUserHistoryAsync(KalowId userId, KalowId configurationId, int pageSize = 20, int page = 0);

    }
}