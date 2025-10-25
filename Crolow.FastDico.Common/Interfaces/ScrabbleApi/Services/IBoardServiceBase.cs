using Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi;

namespace Crolow.FastDico.Common.Interfaces.ScrabbleApi.Services
{
    public interface IBoardServiceBase
    {
        Task<List<IBoardGridModel>> LoadAllAsync();
        void UpdateAsync(IBoardGridModel boardGrid);
    }
}