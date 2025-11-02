using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi;

namespace Crolow.FastDico.Common.Interfaces.ScrabbleApi
{
    public interface IScrabbleAIViewer
    {
        Task<IGameDetailModel> SerializeGame(bool storeGame = true);
        void SerializeUser();
        void ExportHtml(bool doConsole = true);

        PlayedRounds ReplayRound(int round, int max = 300);
    }
}