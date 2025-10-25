using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.Common.Models.ScrabbleApi.Kpi;
using Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi;
using Crolow.FastDico.ScrabbleApi.Extensions;
using Kalow.Apps.Common.DataTypes;

namespace Crolow.FastDico.Builders.TopMachine
{
    public class GameSerializer : IGameSerializer
    {
        public IGameDetailModel GetGame(CurrentGame game)
        {
            GameDetailModel model = new GameDetailModel
            {
                Id = KalowId.NewObjectId(),
                GameConfigurationId = game.GameObjects.Configuration.SelectedConfig.Id,
                ConfigurationName = game.GameObjects.Configuration.SelectedConfig.Name,
                DateTime = DateTime.UtcNow,
                PlayTime = game.GameObjects.Rounds.PlayTime,
                TotalPoints = game.GameObjects.Rounds.TotalPoints,
                Users = new List<KalowId>(), // To be filled
                Rounds = new List<IPlayableSolutionModel>(),
                EditState = Crolow.TopMachine.Data.Bridge.EditState.New
            };

            int totalPoints = 0;
            float totalTime = 0;

            var tilesUtils = game.ControllersSetup.DictionaryContainer.TilesUtils;
            foreach (var round in game.GameObjects.Rounds.Rounds)
            {
                totalPoints += round.Points;
                totalTime += round.PlayedTime;

                PlayableSolutionModel roundModel = new PlayableSolutionModel
                {
                    PlayedTime = round.PlayedTime,
                    Points = round.Points,
                    Position = round.Position.GetPosition(),
                    Rack = round.Rack.GetString(tilesUtils),
                    TotalPlayedTime = totalTime,
                    TotalPoints = totalPoints,
                    Word = round.GetWord(tilesUtils),
                    KpiRate = round.KpiRate
                };

                model.Rounds.Add(roundModel);
            }

            return model;
        }

        public IGameUserDetailModel GetGameUser(IGameDetailModel gameModel, CurrentGame game)
        {
            var userRound = game.GameObjects.UserRounds;
            var kpiRateSummary = new KpiRateSummary();
            GameUserDetailModel model = new GameUserDetailModel
            {
                Id = KalowId.NewObjectId(),
                GameConfigurationId = game.GameObjects.Configuration.SelectedConfig.Id,

                DateTime = DateTime.UtcNow,
                PlayTime = userRound.PlayTime,
                GamePoints = gameModel.TotalPoints,
                TotalPoints = userRound.TotalPoints,
                Rounds = new List<IPlayableSolutionModel>(),
                EditState = Crolow.TopMachine.Data.Bridge.EditState.New,
                GameId = gameModel.Id,
                User = userRound.User.UserId,
                UserName = userRound.User.UserName,
            };

            var kpi = gameModel.Rounds.Select(p => new KpiRate(p.KpiRate)).Take(gameModel.Rounds.Count);
            var suc = new List<bool>();

            int totalPoints = 0;
            float totalTime = 0;
            int missedRounds = 0;
            var tilesUtils = game.ControllersSetup.DictionaryContainer.TilesUtils;
            for (var x = 0; x < gameModel.Rounds.Count; x++)
            {
                var ground = gameModel.Rounds[x];
                if (x < game.GameObjects.UserRounds.Rounds.Count)
                {
                    var round = game.GameObjects.UserRounds.Rounds[x];
                    suc.Add(ground.Points == round.Points);

                    if (round.Points < ground.Points)
                    {
                        missedRounds++;
                    }
                    totalPoints += round.Points;
                    totalTime += round.PlayedTime;

                    PlayableSolutionModel roundModel = new PlayableSolutionModel
                    {
                        PlayedTime = round.PlayedTime,
                        Points = round.Points,
                        Position = round.Position.GetPosition(),
                        Rack = round.Rack.GetString(tilesUtils),
                        TotalPlayedTime = totalTime,
                        TotalPoints = totalPoints,
                        Word = round.GetWord(tilesUtils)
                    };
                    model.Rounds.Add(roundModel);
                }
                else
                {
                    missedRounds++;
                }
            }

            KpiRateSummary kpiSummary = new KpiRateSummary();
            kpiSummary.Calculate(kpi.ToList(), suc.ToList());

            model.KpiRateSummary = kpiSummary;
            model.MissedRounds = missedRounds;
            return model;
        }
    }
}
