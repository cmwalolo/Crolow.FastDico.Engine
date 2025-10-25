using Crolow.FastDico.Common.Interfaces.Users;
using Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi;
using Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi;

namespace Crolow.FastDico.Common.Models.ScrabbleApi;


public class ToppingConfigurationContainer
{
    public ToppingConfigurationContainer(IGameConfigModel game, IBoardGridModel board, IUserClient client = null)
    {
        GameConfig = game;
        BoardGrid = board;
        UserClient = client;
    }

    public IGameConfigModel GameConfig { get; set; }
    public IBoardGridModel BoardGrid { get; set; }
    public IUserClient UserClient { get; set; }
    public bool IsValid { get; set; }
    public int Count { get; set; }

    public IGameUserStatisticsModel Statistics { get; set; }
}

public class StatisticContainer
{
    public string Name { get; set; }
    public IGameUserStatisticsModel Value { get; set; }
}
