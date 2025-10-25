using Crolow.TopMachine.Data.Bridge.Entities.Definitions;
using Crolow.TopMachine.Data.Bridge.Entities.Dictionaries;

namespace Crolow.FastDico.Common.Interfaces.Dictionaries.Services
{
    public interface IDictionaryServiceBase
    {
        Task<IDictionaryContainer> LoadDictionaryAsync(IDictionaryContainer dicoContainer);
        Task<List<IDictionaryModel>> LoadAllAsync();
        void UpdateAsync(IDictionaryModel album);
        Task<List<IWordEntryModel>> GetDefinitionsAsync(string language, string word);
        Task<List<IWordEntryModel>> GetAllEntries(string language);
        Task<List<IWordToDicoModel>> GetAllWordsAsync(string language);
        void UpdateEntriesAsync(string language, List<IWordEntryModel> wordEntryModels);
        void UpdateWordsAsync(string language, List<IWordToDicoModel> wordToDicoModels);
    }
}