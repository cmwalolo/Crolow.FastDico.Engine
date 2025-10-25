using Crolow.FastDico.Common.Interfaces.Users;

namespace Crolow.FastDico.Common.Models.ScrabbleApi.Game;

public class GameDetail
{
    public IUserClient User { get; set; }
    public List<PlayableSolution> Rounds { get; set; } = new List<PlayableSolution>();
    public int TotalPoints { get; set; }
    public float PlayTime { get; set; }

    public GameDetail(IUserClient user)
    {
        this.User = user;
    }
}
