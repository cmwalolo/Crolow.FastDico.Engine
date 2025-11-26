using Crolow.TopMachine.Data.Bridge.Enums;
using Kalow.Apps.Common.DataTypes;

namespace Crolow.TopMachine.Data.Bridge.Entities.Lists
{
    public interface IListStats
    {
        int Count { get; set; }
        int Found { get; set; }
        int NotFound { get; set; }
        int Isolated { get; set; }
    }


    public interface IListConfigurationModel : IDataObject
    {
        public KalowId DictionaryId { get; set; }
        public KalowId UserId { get; set; }
        public IListStats Stats { get; set; }

        List<IListCriteria> Criteria { get; set; }
        string ExcludedLetters { get; set; }
        string MandatoryLetters { get; set; }
        int GameMaxTimeByTurn { get; set; }
        bool GamePause { get; set; }
        int GameRemoveFromNotFound { get; set; }
        string Language { get; set; }
        string LetterForJokers { get; set; }
        string LettersInRack { get; set; }
        bool MatchAllCriteria { get; set; }
        int MaxPossilibies { get; set; }
        int MaxWordLength { get; set; }
        int MinPossilibies { get; set; }
        int MinWordLength { get; set; }
        string Name { get; set; }
        public int NumberOfTurnsBySession { get; set; }
        public int NumberOfRacksByTurn { get; set; }

        TypeOfGame TypeOfGame { get; set; }
        bool UseJokers { get; set; }

        bool IsExtensionList { get; set; }
        int MaxExtensionLetters { get; set; }
        int MinBeforeExtensionLetters { get; set; }
        int BeforeExtensionLetters { get; set; }
        int MinAfterExtensionLetters { get; set; }
        int AfterExtensionLetters { get; set; }
        bool FilterExtensions { get; set; }
        public KalowId ExtensionDictionaryId { get; set; }
    }
}