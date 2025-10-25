using Crolow.FastDico.Common.Models.ScrabbleApi.Entities;
using Kalow.Apps.Common.DataTypes;

namespace Crolow.FastDico.Common.Interfaces.ScrabbleApi.Services
{
    public interface IGameRollerConfigServiceBase
    {
        Task<List<IGameRollerConfigModel>> LoadAllAsync();
        void UpdateAsync(IGameRollerConfigModel gameConfig);
        Task<IGameRollerConfigModel> LoadAsync(KalowId gameConfigId);
    }
}