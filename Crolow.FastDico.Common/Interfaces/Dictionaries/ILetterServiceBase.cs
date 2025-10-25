using Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi;

namespace Crolow.FastDico.Common.Interfaces.Dictionaries
{
    public interface ILetterServiceBase
    {
        Task<List<ILetterConfigModel>> LoadAllAsync();
        void UpdateAsync(ILetterConfigModel gameConfig);
    }
}