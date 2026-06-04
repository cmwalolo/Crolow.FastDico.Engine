using Crolow.FastDico.Builders.TopMachine;
using Crolow.FastDico.Common.Enums;
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
    /// <summary>
    /// Creates new games, loaded games, and view-only game contexts for the topping engine.
    /// </summary>
    public class ToppingFactory : IToppingFactory
    {
        private IDictionaryContainerFactory dictionaryContainerFactory;
        private ITopMachineSetting topMachineSettings;

        private readonly IGameSerializer gameSerializer;
        private readonly IServiceFacadeSwitcher facade;


        /// <summary>
        /// Initializes a new instance of the <see cref="ToppingFactory"/> class.
        /// </summary>
        /// <param name="serviceFactory">Lazy service facade used by game operations.</param>
        /// <param name="dictionaryContainerFactory">Factory used to load dictionary containers.</param>
        /// <param name="iTopMachineSettings">Top-machine settings used by viewers and exports.</param>
        /// <param name="gameSerializer">Serializer used to persist game details.</param>
        /// <param name="facade">Service facade used for persisted games.</param>
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

        /// <summary>
        /// Gets the service facade resolved from the lazy factory.
        /// </summary>
        protected IServiceFacadeSwitcher serviceFacade
        {
            get
            {
                return lazyServiceFactory.Value;
            }
        }

        /// <summary>
        /// Creates and initializes a new current game from a topping configuration.
        /// </summary>
        /// <param name="container">Configuration container that defines the game, board, filters, and dictionaries.</param>
        /// <returns>A fully initialized current game ready to start.</returns>
        public async Task<CurrentGame> CreateGameAsync(ToppingConfigurationContainer container)
        {

            var CurrentGame = new CurrentGame
            {
                //*** Configuration = container,
                ControllersSetup = new GameControllersSetup(),
                GameObjects = new GameObjects()
            };

            CurrentGame.ControllersSetup.DictionaryContainer = await dictionaryContainerFactory.GetContainer(container.GameConfig.Dictionary, null);
            if (container.GameConfig.ReferenceDictionary != null && !container.GameConfig.ReferenceDictionary.Equals(KalowId.Empty))
            {
                CurrentGame.ControllersSetup.ReferenceDictionaryContainer = await dictionaryContainerFactory.GetContainer(container.GameConfig.ReferenceDictionary, null);
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

        /// <summary>
        /// Converts an existing game into a view-ready game by attaching a base validator.
        /// </summary>
        /// <param name="game">Game to convert.</param>
        /// <returns>The same game instance configured for view usage.</returns>
        public async Task<CurrentGame> ConvertToViewAsync(CurrentGame game)
        {
            game.ControllersSetup.Validator = new BaseRoundValidator(game, null);
            return game;
        }


        /// <summary>
        /// Loads a persisted game and rebuilds the runtime controllers needed to replay it.
        /// </summary>
        /// <param name="model">Persisted game detail model.</param>
        /// <param name="detail">Persisted user history for the game.</param>
        /// <param name="configContainer">Configuration container used to rebuild runtime state.</param>
        /// <returns>A current game configured for replay.</returns>
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
            CurrentGame.ControllersSetup.DictionaryContainer = await dictionaryContainerFactory.GetContainer(configContainer.GameConfig.Dictionary, null);
            var playConfiguration = new ConfigLoader().ReadConfiguration(configContainer, CurrentGame.ControllersSetup.DictionaryContainer);
            CurrentGame.ControllersSetup.DictionaryContainer = await dictionaryContainerFactory.GetContainer(configContainer.GameConfig.Dictionary, null);

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
