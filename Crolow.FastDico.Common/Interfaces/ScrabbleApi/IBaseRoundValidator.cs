using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.ScrabbleApi.Components.BoardSolvers;

namespace Crolow.FastDico.Common.Interfaces.ScrabbleApi
{
    public interface IBaseRoundValidator
    {
        bool CanRejectBagByDefault(LetterBag bag, List<Tile> rack);
        PlayableSolution FinalizeRound(PlayedRounds playedRounds);
        void Initialize();
        void InitializeRound();
        PlayedRounds GetRound(List<Tile> letters, SolverFilters filters = null);
        SolverFilters InitializeFilters(bool pickAll = false);
        List<Tile> InitializeLetters(List<Tile> rack);
        PlayedRounds ValidateRound(PlayedRounds rounds, List<Tile> letters, IBoardSolver solver);
        bool IsValidGame();
    }
}