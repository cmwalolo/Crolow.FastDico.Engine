using Crolow.FastDico.Common.Interfaces.ScrabbleApi;
using Crolow.FastDico.Common.Interfaces.ScrabbleApi.Services;
using Crolow.FastDico.Common.Interfaces.Settings;
using Crolow.FastDico.Common.Models.ScrabbleApi;
using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Kalow.Apps.Common.Utils;
using Microsoft.Extensions.Hosting;

namespace Crolow.TopMachine.Builders.TopMachine
{
    public class GameGenerator : BackgroundService, IDisposable
    {
        public static GameGenerator Instance;

        private ITopMachineSetting topMachineSetting;
        public bool IsRunning { get; set; }

        public IToppingFactory ToppingFactory { get; set; }
        public IDictionaryContainerFactory DictionaryContainerFactory { get; set; }

        public GameGenerator(ITopMachineSetting topMachineSetting,
             Lazy<IServiceFacadeSwitcher> serviceFactory,
            IToppingFactory toppingFactory,
            IDictionaryContainerFactory dictionaryContainerFactory
            )
        {

            this.topMachineSetting = topMachineSetting;
            lazyServiceFactory = serviceFactory; // generate new instance because we need to have only a serviceLocal
            ToppingFactory = toppingFactory;
            this.DictionaryContainerFactory = dictionaryContainerFactory;
        }

        Lazy<IServiceFacadeSwitcher> lazyServiceFactory;

        protected IServiceFacadeSwitcher ServiceFacadeSwitcher
        {
            get
            {
                return lazyServiceFactory.Value;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var dicoPath = topMachineSetting.OutputFolderPath;
            var path = topMachineSetting.OutputFolderPath;
            var gameConfigs = await ServiceFacadeSwitcher.Current.GameConfigService.LoadAllAsync();
            var boards = await ServiceFacadeSwitcher.Current.BoardService.LoadAllAsync();
            var letters = await ServiceFacadeSwitcher.Current.LetterService.LoadAllAsync();
            var dicos = await ServiceFacadeSwitcher.Current.DictionaryService.LoadAllAsync();
            IsRunning = true;
            while (!stoppingToken.IsCancellationRequested)
            {
                CurrentGame game = null;
                try
                {
                    foreach (var gameConfig in gameConfigs
                        //.Where(p => p.Name.Contains("Normale"))
                        .OrderBy(p => p.Id))
                    {
                        ToppingConfigurationContainer container = new ToppingConfigurationContainer(gameConfig, boards.FirstOrDefault(p => gameConfig.BoardConfig == p.Id));

                        if (container.BoardGrid != null && container.BoardGrid != null)
                        {
                            container.IsValid = true;
                        }

                        if (container.IsValid)
                        {
                            var factory = ToppingFactory;
                            var maxcpt = 3;
                            var cpt = await ServiceFacadeSwitcher.Current.GameService.CountPoolAsync(gameConfig.Id);
                            for (int i = 0; i < maxcpt - cpt; i++)
                            {
                                await Task.Run(async () =>
                                {
                                    game = await InitializeGameAsync(container);
                                    game = await GenerateGameAsync(game);
                                    game.ControllersSetup.ScrabbleViewEngine.SerializeGame(true);

                                    if (game != null)
                                    {
                                        game = null;
                                        GC.Collect();
                                        GC.WaitForPendingFinalizers();
                                        GC.Collect();
                                    }
                                }, stoppingToken);
                            }
                        }
                    }

                    if (!stoppingToken.IsCancellationRequested)
                    {
                        // Simulate periodic work
                        try
                        {
                            await Task.Delay(5000, stoppingToken);
                        }
                        catch (TaskCanceledException) when (stoppingToken.IsCancellationRequested)
                        {
                            ;// Ignore
                        }
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // Expected cancellation - ignore
                }
                catch (Exception ex)
                {
                    BufferedConsole.WriteLine(ex.Message, ex);
                }
                finally
                {
                    if (game != null)
                    {
                        game = null;
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();
                    }
                }
            }
        }


        public async Task<CurrentGame> InitializeGameAsync(ToppingConfigurationContainer container)
        {
            BufferedConsole.ClearBuffer();
            CurrentGame game = await ToppingFactory.CreateGameAsync(container);
            return game;
        }

        public async Task<CurrentGame> GenerateGameAsync(CurrentGame game)
        {
            await game.ControllersSetup.ScrabbleEngine.StartGame();
            return game;
        }

        public static void Start()
        {
            Task.Run(() => Instance.StartAsync(CancellationToken.None));
        }

        public static void Stop()
        {
            Instance?.StopAsync(CancellationToken.None);
        }


        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            IsRunning = false;
            await base.StopAsync(stoppingToken);
        }
        public override void Dispose()
        {
            ServiceFacadeSwitcher.Current.DictionaryService = null;
            ServiceFacadeSwitcher.Current.GameConfigService = null;
            ServiceFacadeSwitcher.Current.BoardService = null;
            ServiceFacadeSwitcher.Current.LetterService = null;
        }
    }
}
