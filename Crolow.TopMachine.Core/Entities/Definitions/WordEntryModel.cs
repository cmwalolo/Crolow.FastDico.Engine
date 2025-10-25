using Crolow.TopMachine.Data.Bridge.DataObjects;
using Crolow.TopMachine.Data.Bridge.Entities.Definitions;
using Newtonsoft.Json;

namespace Crolow.TopMachine.Core.Entities.Definitions
{
    public class WordEntryModel : DataObject, IWordEntryModel
    {
        public string Word { get; set; }
        public string NormalizedWord { get; set; }
        public string Source { get; set; }

        [JsonProperty("ethymology")]
        public string Etymology { get; set; }

        public List<IDefinitionModel> Definitions { get; set; } = new List<IDefinitionModel>();

        public int RateComplexity { get; set; }
        public int RatePopularity { get; set; }
        public int RateGlobal { get; set; }
        public int DifficultyLevel { get; set; }

    }
}
