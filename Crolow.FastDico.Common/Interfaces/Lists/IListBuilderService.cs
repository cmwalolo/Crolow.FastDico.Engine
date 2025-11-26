using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.TopMachine.Data.Bridge.Entities.Lists;

namespace Crolow.FastDico.Common.Interfaces.Lists
{
    public interface IListBuilderService
    {
        Task BuildAsync(IListConfigurationModel config, IDictionaryContainer dicoContainer, IDictionaryContainer dicoContainerRef);
    }
}