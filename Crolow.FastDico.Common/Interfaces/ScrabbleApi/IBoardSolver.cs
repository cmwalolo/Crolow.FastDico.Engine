using Crolow.FastDico.Common.Models.ScrabbleApi.Game;

namespace Crolow.FastDico.ScrabbleApi.Components.BoardSolvers
{
    public interface IBoardSolver
    {
        void Initialize();
        PlayedRounds Solve(List<Tile> letters, SolverFilters filters = null);
        bool ValidateRound(PlayableSolution solution);
    }
}