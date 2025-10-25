using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Utils;
using Crolow.TopMachine.Data.Bridge.Entities.Dictionaries;
using Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi;

namespace Crolow.FastDico.Common.Models.Dictionary
{
    public class DictionaryContainer : IDictionaryContainer
    {
        public ILetterConfigModel LetterConfig { get; set; }
        public IDictionaryModel Dictionary { get; set; }

        public IBaseDictionary Dico { get; set; }
        public ITileConfiguration TileConfiguration { get; set; }
        public ITilesUtils TilesUtils { get; set; }
        public IDawgSearch Searcher { get; set; }

    }
}
