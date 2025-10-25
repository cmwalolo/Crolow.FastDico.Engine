using Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi;

namespace Crolow.FastDico.Common.Models.ScrabbleApi.Game
{
    public class PlayedGame
    {
        public IGameDetailModel GameDetail { get; set; }
        public List<IGameUserDetailModel> UserDetails { get; set; } = new List<IGameUserDetailModel>();
        //*** public ToppingConfigurationContainer Configuration { get; set; }
    }
}
