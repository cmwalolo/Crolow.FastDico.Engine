using Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi;

namespace Crolow.FastDico.Common.Interfaces.ScrabbleApi
{
    public interface IBoardServiceBase
    {
        Task<List<IBoardGridModel>> LoadAllAsync();
        void UpdateAsync(IBoardGridModel boardGrid);
    }
}