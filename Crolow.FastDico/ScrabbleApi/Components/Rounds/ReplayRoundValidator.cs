using Crolow.FastDico.Common.Interfaces.ScrabbleApi;
using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi;
using Crolow.FastDico.ScrabbleApi.Components.BoardSolvers;
using Crolow.FastDico.ScrabbleApi.Extensions;

namespace Crolow.FastDico.ScrabbleApi.Components.Rounds
{
    public class ReplayRoundValidator : BaseRoundValidator, IBaseRoundValidator
    {
        private IGameDetailModel game;
        private List<IGameUserDetailModel> history;
        public ReplayRoundValidator(CurrentGame currentGame, IGameDetailModel game) : base(currentGame, null)
        {
            this.game = game;
        }

        public virtual void Initialize()
        {

        }
        public virtual void InitializeRound()
        {
        }

        public virtual List<Tile> InitializeLetters(List<Tile> tiles)
        {
            if (currentGame.GameObjects.Round < game.Rounds.Count)
            {
                var round = game.Rounds[currentGame.GameObjects.Round];
                var letters = currentGame.GameObjects.GameLetterBag.ForceDrawLetters(currentGame, round.Rack);
                return letters;
            }

            return null;
        }

        public virtual PlayedRounds GetRound(List<Tile> letters, SolverFilters filters = null)
        {
            return base.GetRound(letters, filters);
        }

        public virtual PlayedRounds ValidateRound(PlayedRounds rounds, List<Tile> letters, IBoardSolver solver)
        {
            return rounds;
        }

        public virtual bool CanRejectBagByDefault(LetterBag bag, PlayerRack rack)
        {
            return false;
        }

        public virtual SolverFilters InitializeFilters(bool pickAll = false)
        {
            Filters.PickallResults = pickAll;
            return Filters;
        }

        /// <param name="playedRounds"></param>
        /// <returns></returns>
        public virtual PlayableSolution FinalizeRound(PlayedRounds playedRounds)
        {
            if (playedRounds.Tops.Count == 0)
            {
                return null;
            }
            try
            {
                var round = game.Rounds[currentGame.GameObjects.Round];
                var selectedRound = playedRounds.Tops.FirstOrDefault(p => p.IsEqual(round, currentGame.ControllersSetup.DictionaryContainer.TilesUtils, true));
                selectedRound.Rack = new PlayerRack(playedRounds.PlayerRack);
                selectedRound.FinalizeRound();
                return selectedRound;
            }
            catch (Exception)
            {
                throw;
            }

        }

    }
}
