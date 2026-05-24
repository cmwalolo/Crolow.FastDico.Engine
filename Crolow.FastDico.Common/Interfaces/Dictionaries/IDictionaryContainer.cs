using Crolow.FastDico.Utils;
using Crolow.TopMachine.Data.Bridge.Entities.Dictionaries;
using Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi;

namespace Crolow.FastDico.Common.Interfaces.Dictionaries
{
    public interface IDictionaryContainer
    {
        IBaseDictionary Dico { get; set; }
        IDictionaryModel Dictionary { get; set; }
        ILetterConfigModel LetterConfig { get; set; }
        IDicoSearch Searcher { get; set; }
        ITilesUtils TilesUtils { get; set; }
        ITilesConfiguration TileConfiguration { get; set; }
    }

}