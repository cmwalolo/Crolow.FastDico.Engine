using Crolow.FastDico.Common.Models.ScrabbleApi;
using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi;

namespace Crolow.FastDico.Common.Interfaces.ScrabbleApi
{
    public interface IToppingFactory
    {
        Task<CurrentGame> LoadGameAsync(IGameDetailModel model, List<IGameUserDetailModel> detail, ToppingConfigurationContainer container);
        Task<CurrentGame> CreateGameAsync(ToppingConfigurationContainer container);
        Task<CurrentGame> ConvertToViewAsync(CurrentGame game);
    }
}