using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Common.Interfaces.ScrabbleApi;
using Crolow.FastDico.Common.Interfaces.ScrabbleApi.Services;
using Crolow.FastDico.Common.Interfaces.Settings;
using Crolow.FastDico.Common.Models.Dictionary;
using Crolow.FastDico.ScrabbleApi.Config;
using Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi;
using Kalow.Apps.Common.DataTypes;

namespace Crolow.FastDico.ScrabbleApi.Factories
{
    public class DictionaryContainerFactory : IDictionaryContainerFactory
    {
        private ITopMachineSetting topMachineSettings;
        protected static Dictionary<KalowId, IDictionaryContainer> cache = new Dictionary<KalowId, IDictionaryContainer>();
        public DictionaryContainerFactory(Lazy<IServiceFacadeSwitcher> serviceFactory, ITopMachineSetting iTopMachineSettings)
        {
            this.lazyServiceFactory = serviceFactory;
            this.topMachineSettings = iTopMachineSettings;
        }

        Lazy<IServiceFacadeSwitcher> lazyServiceFactory;

        protected IServiceFacadeSwitcher serviceFacade
        {
            get
            {
                return lazyServiceFactory.Value;
            }
        }



        public async Task<IDictionaryContainer> GetContainer(KalowId dictionaryId, string language)
        {

            if (cache.ContainsKey(dictionaryId))
            {
                return cache[dictionaryId];
            }
            else
            {
                var container = new DictionaryContainer();
                var model = (await serviceFacade.Current.DictionaryService.LoadAllAsync()).FirstOrDefault(p => p.Id == dictionaryId || (p.Language == language && p.IsDefault && dictionaryId.Equals(KalowId.Empty)));
                dictionaryId = model?.Id ?? KalowId.Empty;

                if (cache.ContainsKey(dictionaryId))
                {
                    return cache[dictionaryId];
                }

                container.Dictionary = model;
                container.LetterConfig = (await serviceFacade.Current.LetterService.LoadAllAsync()).FirstOrDefault(predicate: p => p.Id == model.LetterConfig);

                await serviceFacade.Current.DictionaryService.LoadDictionaryAsync(container);

                container.TileConfiguration = ReadLetterConfig(container.LetterConfig);
                container.TilesUtils = new Crolow.FastDico.Utils.TilesUtils(container.TileConfiguration);
                if (container.Dico != null)
                {
                    container.Searcher = new Crolow.FastDico.GadDag.GadDagSearch(container.Dico.Root, container.TilesUtils);
                }
                cache[model.Id] = container;
                return container;
            }
        }

        private TileConfiguration ReadLetterConfig(ILetterConfigModel letterData)
        {
            var config = new TileConfiguration();

            config.Name = letterData.Name;
            foreach (var letter in letterData.Letters)
            {
                config.LettersByByte.Add(letter.Letter, letter);
                config.LettersByChar.Add(letter.Char[0], letter);
            }
            return config;
        }

    }
}
