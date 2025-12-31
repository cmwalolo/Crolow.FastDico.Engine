using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Kalow.Apps.Common.DataTypes;

namespace Crolow.FastDico.Common.Interfaces.ScrabbleApi
{
    public interface IDictionaryContainerFactory
    {
        Task<IDictionaryContainer> GetContainer(KalowId dictionaryId, string language);
    }
}