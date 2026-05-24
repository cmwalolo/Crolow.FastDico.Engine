using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi;

namespace Crolow.FastDico.ScrabbleApi.Config
{
    public partial class PlayConfiguration
    {
        public ITilesConfiguration TileConfig { get; set; }
        public GridConfigurationContainer GridConfig { get; set; }

        public IGameConfigModel SelectedConfig { get; set; }

    }
}
