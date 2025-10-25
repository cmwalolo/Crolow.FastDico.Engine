using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.TopMachine.Data.Bridge.Entities.Definitions;

namespace Crolow.TopMachine.Core.Service.Services.Dictionary
{
    public interface IDictionaryBuilderService
    {
        void GenerateDictionary(IDictionaryContainer dictionaryContainer, List<IWordEntryModel> entries, List<IWordToDicoModel> words, string outputFilename, bool doEntries, bool doNocNop, bool doNoc);
        void ApplyTypes(List<IWordEntryModel> entries, List<IWordToDicoModel> words);
    }
}