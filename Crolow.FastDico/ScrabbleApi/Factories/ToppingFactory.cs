using Crolow.FastDico.Builders.TopMachine;
using Crolow.FastDico.Common.Interfaces.ScrabbleApi;
using Crolow.FastDico.Common.Interfaces.ScrabbleApi.Services;
using Crolow.FastDico.Common.Interfaces.Settings;
using Crolow.FastDico.Common.Interfaces.Users;
using Crolow.FastDico.Common.Models.ScrabbleApi;
using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi;
using Crolow.FastDico.ScrabbleApi.Components.BoardSolvers;
using Crolow.FastDico.ScrabbleApi.Components.Rounds;
using Crolow.FastDico.ScrabbleApi.Utils;
using Kalow.Apps.Common.DataTypes;

namespace Crolow.FastDico.ScrabbleApi.Factories
{
    public class ToppingFactory : IToppingFactory
    {
        private IDictionaryContainerFactory dictionaryContainerFactory;
        private ITopMachineSetting topMachineSettings;

        private readonly IGameSerializer gameSerializer;
        private readonly IServiceFacadeSwitcher facade;


        public ToppingFactory(Lazy<IServiceFacadeSwitcher> serviceFactory,
                              IDictionaryContainerFactory dictionaryContainerFactory,
                              ITopMachineSetting iTopMachineSettings,
                              IGameSerializer gameSerializer,
                              IServiceFacadeSwitcher facade)
        {
            this.lazyServiceFactory = serviceFactory;
            this.dictionaryContainerFactory = dictionaryContainerFactory;
            this.topMachineSettings = iTopMachineSettings;
            this.gameSerializer = gameSerializer;
            this.facade = facade;
        }

        Lazy<IServiceFacadeSwitcher> lazyServiceFactory;

        protected IServiceFacadeSwitcher serviceFacade
        {
            get
            {
                return lazyServiceFactory.Value;
            }
        }

        public async Task<CurrentGame> CreateGameAsync(ToppingConfigurationContainer container)
        {

            var CurrentGame = new CurrentGame
            {
                //*** Configuration = container,
                ControllersSetup = new GameControllersSetup(),
                GameObjects = new GameObjects()
            };

            CurrentGame.ControllersSetup.DictionaryContainer = await dictionaryContainerFactory.GetContainer(container.GameConfig.Dictionary);
            if (container.GameConfig.ReferenceDictionary != null && !container.GameConfig.ReferenceDictionary.Equals(KalowId.Empty))
            {
                CurrentGame.ControllersSetup.ReferenceDictionaryContainer = await dictionaryContainerFactory.GetContainer(container.GameConfig.ReferenceDictionary);
            }

            var playConfiguration = new ConfigLoader().ReadConfiguration(container, CurrentGame.ControllersSetup.DictionaryContainer);


            var user = IUserClientContext.Instance;

            CurrentGame.GameObjects.Rounds = new GameDetail(null);
            if (user != null)
            {
                CurrentGame.GameObjects.UserRounds = new GameDetail(container.UserClient);
            }

            CurrentGame.GameObjects.Configuration = playConfiguration;
            CurrentGame.GameObjects.GameConfig = playConfiguration.SelectedConfig;
            CurrentGame.GameObjects.MaxRounds = container.MaxRounds;
            CurrentGame.GameObjects.Board = new Board(CurrentGame.GameObjects);
            CurrentGame.GameObjects.GameLetterBag = new LetterBag(CurrentGame.GameObjects);
            CurrentGame.GameObjects.GameRack = new PlayerRack();
            CurrentGame.GameObjects.LetterConfig = CurrentGame.ControllersSetup.DictionaryContainer.LetterConfig;

            CurrentGame.ControllersSetup.PivotBuilder = new PivotBuilder(CurrentGame);
            CurrentGame.ControllersSetup.BoardSolver = new BoardSolver(CurrentGame);

            if (CurrentGame.GameObjects.GameConfig.DifficultMode)
            {
                CurrentGame.ControllersSetup.Validator = new XRoundValidator(CurrentGame, container.Filters);
            }
            else
            {
                CurrentGame.ControllersSetup.Validator = new BaseRoundValidator(CurrentGame, container.Filters);
            }

            CurrentGame.ControllersSetup.ScrabbleEngine = new ScrabbleAI(CurrentGame);
            CurrentGame.ControllersSetup.ScrabbleViewEngine = new ScrabbleAIViewer(null, CurrentGame, topMachineSettings, gameSerializer, serviceFacade);
            CurrentGame.GameObjects.GameStatus = GameStatus.WaitingToStart;
            return CurrentGame;
        }

        public async Task<CurrentGame> ConvertToViewAsync(CurrentGame game)
        {
            game.ControllersSetup.Validator = new BaseRoundValidator(game, null);
            return game;
        }


        public async Task<CurrentGame> LoadGameAsync(IGameDetailModel model, List<IGameUserDetailModel> detail, ToppingConfigurationContainer configContainer)
        {
            var gameConfig = serviceFacade.Current.GameConfigService.LoadAsync(model.GameConfigurationId);

            var CurrentGame = new CurrentGame
            {
                ControllersSetup = new GameControllersSetup(),
                GameObjects = new GameObjects()
            };
            CurrentGame.History.Game = model;
            CurrentGame.History.Users = detail;
            CurrentGame.ControllersSetup.DictionaryContainer = await dictionaryContainerFactory.GetContainer(configContainer.GameConfig.Dictionary);
            var playConfiguration = new ConfigLoader().ReadConfiguration(configContainer, CurrentGame.ControllersSetup.DictionaryContainer);
            CurrentGame.ControllersSetup.DictionaryContainer = await dictionaryContainerFactory.GetContainer(configContainer.GameConfig.Dictionary);

            CurrentGame.GameObjects.MaxRounds = configContainer.MaxRounds;
            CurrentGame.GameObjects.Rounds = new GameDetail(null);
            CurrentGame.GameObjects.UserRounds = new GameDetail(configContainer.UserClient);

            CurrentGame.GameObjects.Configuration = playConfiguration;
            CurrentGame.GameObjects.GameConfig = playConfiguration.SelectedConfig;

            CurrentGame.GameObjects.Board = new Board(CurrentGame.GameObjects);
            CurrentGame.GameObjects.GameLetterBag = new LetterBag(CurrentGame.GameObjects);
            CurrentGame.GameObjects.GameRack = new PlayerRack();
            CurrentGame.GameObjects.LetterConfig = CurrentGame.ControllersSetup.DictionaryContainer.LetterConfig;

            CurrentGame.ControllersSetup.PivotBuilder = new PivotBuilder(CurrentGame);
            CurrentGame.ControllersSetup.BoardSolver = new BoardSolver(CurrentGame);

            CurrentGame.ControllersSetup.Validator = new ReplayRoundValidator(CurrentGame, model);
            CurrentGame.ControllersSetup.ScrabbleEngine = new ScrabbleAI(CurrentGame);
            CurrentGame.ControllersSetup.ScrabbleViewEngine = new ScrabbleAIViewer(model, CurrentGame, topMachineSettings, gameSerializer, facade);
            CurrentGame.GameObjects.GameStatus = GameStatus.WaitingToStart;
            return CurrentGame;
        }

    }
}
