using Crolow.TopMachine.Data.Bridge.DataObjects;
using Crolow.TopMachine.Data.Bridge.Entities.Lists;
using Crolow.TopMachine.Data.Bridge.Enums;
using Kalow.Apps.Common.DataTypes;

namespace Crolow.TopMachine.Core.Entities.Lists
{
    public class ListItemModel : DataObject, IListItemModel
    {
        public KalowId ListId { get; set; } = KalowId.Empty;
        public string Rack { get; set; } = string.Empty;
        public List<string> Solutions { get; set; }
        public StatusOfListItem Status { get; set; } = StatusOfListItem.NotPlayed;
        public int FoundCount { get; set; } = 0;
        public int NotFoundCount { get; set; } = 0;
        public int FoundInRowCount { get; set; } = 0;
    }

    public class ListStats : IListStats
    {
        public int Count { get; set; }
        public int Found { get; set; }
        public int NotFound { get; set; }
        public int Isolated { get; set; }
    }

    public class ListConfigurationModel : DataObject, IListConfigurationModel
    {
        public KalowId DictionaryId { get; set; } = KalowId.Empty;
        public KalowId UserId { get; set; } = KalowId.Empty;

        #region Configuration
        public string? Name { get; set; } = string.Empty;
        public string? Language { get; set; } = string.Empty;
        public string? LettersInRack { get; set; } = string.Empty;
        public string? ExcludedLetters { get; set; } = string.Empty;
        public string? MandatoryLetters { get; set; } = string.Empty;
        public int MinWordLength { get; set; }
        public int MaxWordLength { get; set; }
        public int MinPossilibies { get; set; }
        public int MaxPossilibies { get; set; }
        public bool UseJokers { get; set; }
        public string? LetterForJokers { get; set; } = string.Empty;
        public bool MatchAllCriteria { get; set; } = false;
        public List<IListCriteria> Criteria { get; set; } = new List<IListCriteria>();

        #endregion


        #region Parameters
        public int GameRemoveFromNotFound { get; set; }
        public int GameMaxTimeByTurn { get; set; }
        public bool GamePause { get; set; } = false;
        public TypeOfGame TypeOfGame { get; set; } = TypeOfGame.AllWords;
        public int NumberOfTurnsBySession { get; set; } = 50;
        public int NumberOfRacksByTurn { get; set; } = 1;

        #endregion

        public IListStats Stats { get; set; } = new ListStats();
    }
}