using Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi;

namespace Crolow.FastDico.Common.Interfaces.Dictionaries.Services
{
    public interface ILetterServiceBase
    {
        Task<List<ILetterConfigModel>> LoadAllAsync();
        void UpdateAsync(ILetterConfigModel gameConfig);
    }
}