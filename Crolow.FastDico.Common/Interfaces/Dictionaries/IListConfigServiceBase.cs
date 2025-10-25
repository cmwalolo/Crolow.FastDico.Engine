using Crolow.TopMachine.Core.Entities.Lists;
using Crolow.TopMachine.Data.Bridge.Entities.Lists;

namespace Crolow.FastDico.Common.Interfaces.Dictionaries
{
    public interface IListConfigServiceBase
    {
        Task<List<IListConfigurationModel>> LoadAllListsConfigAsync();
        Task UpdateListConfigAsync(IListConfigurationModel gameConfig);
        Task<List<IListItemModel>> LoadItemsAsync(IListConfigurationModel gameConfig);
        Task UpdateItemsAsync(List<IListItemModel> items);
    }

    public interface IListConfigServiceLocal : IListConfigServiceBase
    {

    }

    public interface IListConfigServiceRemote : IListConfigServiceBase
    {

    }
}