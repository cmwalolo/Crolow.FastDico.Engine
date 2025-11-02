using Crolow.FastDico.Common.Interfaces.Dictionaries.Services;

namespace Crolow.FastDico.Common.Interfaces.ScrabbleApi.Services
{
    public interface IServiceFacadeLocal : IServiceFacade
    {

    }

    public interface IServiceFacadeRemote : IServiceFacade
    {
    }

    public interface IServiceFacadeServer : IServiceFacade
    {

    }


    public interface IServiceFacade
    {
        public IListConfigServiceBase ListConfigService { get; set; }
        public IGameConfigServiceBase GameConfigService { get; set; }
        public IBoardServiceBase BoardService { get; set; }
        public ILetterServiceBase LetterService { get; set; }
        public IDictionaryServiceBase DictionaryService { get; set; }
        public IGameServiceBase GameService { get; set; }
        public IGameRollerConfigServiceBase GameRollerConfigService { get; set; }


    }
}