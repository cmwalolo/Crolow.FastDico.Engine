using Crolow.FastDico.Common.Models.ScrabbleApi.Game;

namespace Crolow.FastDico.Common.Interfaces.ScrabbleApi
{

    public interface IScrabbleAI
    {
        public delegate void RoundIsReadyEvent();
        public delegate void RoundSelectedEvent(PlayableSolution solution, PlayerRack rack);
        public delegate void GameEndedEvent();

        event RoundIsReadyEvent RoundIsReady;
        event RoundSelectedEvent RoundSelected;
        event GameEndedEvent GameEnded;

        void EndGame();
        Task<bool> NextRound();
        Task StartGame();
        void SetRound(PlayableSolution userSolution = null);
        Task<int> ValidateRound(PlayableSolution solution, bool forceTop);
        Task<bool> FinalizeRound(PlayableSolution solution, bool forceTop);


    }
}