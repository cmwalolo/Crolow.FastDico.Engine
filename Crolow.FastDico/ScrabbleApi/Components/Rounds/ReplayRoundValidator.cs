using Crolow.FastDico.Common.Interfaces.ScrabbleApi;
using Crolow.FastDico.Common.Models.ScrabbleApi.Game;
using Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi;
using Crolow.FastDico.ScrabbleApi.Components.BoardSolvers;
using Crolow.FastDico.ScrabbleApi.Extensions;

namespace Crolow.FastDico.ScrabbleApi.Components.Rounds
{
    /// <summary>
    /// Replays a persisted game by forcing rack draws from the stored round history.
    /// </summary>
    public class ReplayRoundValidator : BaseRoundValidator, IBaseRoundValidator
    {
        private IGameDetailModel game;
        private List<IGameUserDetailModel> history;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplayRoundValidator"/> class.
        /// </summary>
        /// <param name="currentGame">Current game context used for replay.</param>
        /// <param name="game">Persisted game detail model to replay.</param>
        public ReplayRoundValidator(CurrentGame currentGame, IGameDetailModel game) : base(currentGame, null)
        {
            this.game = game;
        }

        /// <summary>
        /// Initializes replay validation state.
        /// </summary>
        public virtual void Initialize()
        {

        }

        /// <summary>
        /// Initializes replay state for a new round.
        /// </summary>
        public virtual void InitializeRound()
        {
        }

        /// <summary>
        /// Forces the rack letters from the persisted round at the current replay index.
        /// </summary>
        /// <param name="tiles">Current rack tiles, unused by replay draws.</param>
        /// <returns>The forced rack letters, or <c>null</c> when replay has no more rounds.</returns>
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

        /// <summary>
        /// Solves a replay round with the supplied letters and filters.
        /// </summary>
        /// <param name="letters">Letters available for the replayed round.</param>
        /// <param name="filters">Optional solver filters.</param>
        /// <returns>Candidate rounds produced by the base solver.</returns>
        public virtual PlayedRounds GetRound(List<Tile> letters, SolverFilters filters = null)
        {
            return base.GetRound(letters, filters);
        }

        /// <summary>
        /// Accepts replay candidate rounds without additional filtering.
        /// </summary>
        /// <param name="rounds">Candidate rounds to accept.</param>
        /// <param name="letters">Letters used to produce the rounds.</param>
        /// <param name="solver">Board solver used for the candidates.</param>
        /// <returns>The supplied rounds.</returns>
        public virtual PlayedRounds ValidateRound(PlayedRounds rounds, List<Tile> letters, IBoardSolver solver)
        {
            return rounds;
        }

        /// <summary>
        /// Determines whether replay should reject the bag by default.
        /// </summary>
        /// <param name="bag">Letter bag to inspect.</param>
        /// <param name="rack">Rack to inspect.</param>
        /// <returns>Always returns <c>false</c> for replay.</returns>
        public virtual bool CanRejectBagByDefault(LetterBag bag, PlayerRack rack)
        {
            return false;
        }

        /// <summary>
        /// Initializes solver filters for replay.
        /// </summary>
        /// <param name="pickAll">Indicates whether all results should be collected.</param>
        /// <returns>The filters to use for solving.</returns>
        public virtual SolverFilters InitializeFilters(bool pickAll = false)
        {
            Filters.PickallResults = pickAll;
            return Filters;
        }

        /// <summary>
        /// Selects the solved round matching the persisted replay round.
        /// </summary>
        /// <param name="playedRounds">Candidate rounds produced during replay.</param>
        /// <returns>The matching finalized round, or <c>null</c> when no top exists.</returns>
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
