namespace Crolow.TopMachine.Core.Extensions
{
    public static class GameDetailExtensions
    {
        /*public static IGameDetailModel ToReadable(this GameDetail gameDetail, ITilesUtils utils)
        {
            GameDetailModel item = new GameDetailModel();

            item.PlayTime = gameDetail.PlayTime;
            item.User = gameDetail.User.Id;
            item.TotalPoints = gameDetail.TotalPoints;
            item.Rounds = gameDetail.Rounds.Select(p =>
                        new PlayableSolutionModel
                        {
                            PlayedTime = p.PlayedTime,
                            Points = p.Points,
                            Position = p.GetPosition(),
                            Rack = p.Rack.GetString(utils),
                            Word = p.GetWord(utils, false),
                        } as IPlayableSolutionModel
             ).ToList();

            return item;
        }*/
    }
}
