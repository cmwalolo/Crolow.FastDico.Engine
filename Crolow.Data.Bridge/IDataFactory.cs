using Crolow.FastDico.Common.Models.ScrabbleApi.Entities;
using Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi;
using Crolow.TopMachine.Core.Entities.Lists;
using Crolow.TopMachine.Data.Bridge.Entities.Definitions;
using Crolow.TopMachine.Data.Bridge.Entities.Dictionaries;
using Crolow.TopMachine.Data.Bridge.Entities.Lists;
using Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi;
using Crolow.TopMachine.Data.Repositories;

namespace Crolow.TopMachine.Data.Bridge;
public interface IDataFactory
{
    IDataManager<IListItemModel> ListItems { get; }
    IDataManager<IListConfigurationModel> ListConfigs { get; }
    IDataManager<ILetterConfigModel> LetterConfigs { get; }
    IDataManager<IBoardGridModel> Boards { get; }
    IDataManager<IGameConfigModel> Games { get; }
    IDataManager<IGameRollerConfigModel> GameRollers { get; }
    IDataManager<IDictionaryModel> Dictionaries { get; }
    IGamePoolDataManager<IGameDetailModel> GamePool { get; }
    IGameUsersDataManager<IGameUserDetailModel> UserGames { get; }
    IGameUserStatisticsDataManager<IGameUserStatisticsModel> UserStatistics { get; }

    public IDictionaryDataFactory GetDictionaryFactory(string language);
}

public interface IDictionaryDataFactory
{
    IDataManager<IWordEntryModel> DicoEntries { get; }
    IDataManager<IWordToDicoModel> DicoWords { get; }
}

