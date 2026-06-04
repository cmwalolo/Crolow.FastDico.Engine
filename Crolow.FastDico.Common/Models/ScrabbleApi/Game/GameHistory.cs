using Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi;

namespace Crolow.FastDico.Common.Models.ScrabbleApi.Game;

public class GameHistory
{
    public IGameDetailModel Game { get; set; }
    public List<IGameUserDetailModel> Users { get; set; } = new List<IGameUserDetailModel>();
}
