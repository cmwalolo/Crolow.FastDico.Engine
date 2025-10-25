using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi;

namespace Crolow.FastDico.Builders.TopMachine
{
    public interface IGameSerializer
    {
        IGameDetailModel GetGame(CurrentGame game);
        IGameUserDetailModel GetGameUser(IGameDetailModel gameModel, CurrentGame game);
    }
}