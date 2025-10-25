using Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi;
using Kalow.Apps.Common.DataTypes;

namespace Crolow.FastDico.Common.Interfaces.ScrabbleApi.Services
{
    public interface IGameConfigServiceBase
    {
        Task<List<IGameConfigModel>> LoadAllAsync();
        void UpdateAsync(IGameConfigModel gameConfig);
        Task<IGameConfigModel> LoadAsync(KalowId gameConfigId);
    }
}