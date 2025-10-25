using Crolow.FastDico.Common.Models.ScrabbleApi.Game;

namespace Crolow.FastDico.Common.Interfaces.ScrabbleApi
{
    public interface IScrabbleAIViewer
    {
        void SerializeGame();
        void SerializeUser();
        void ExportHtml(bool doConsole = true);

        PlayedRounds ReplayRound(int round, int max = 300);
    }
}