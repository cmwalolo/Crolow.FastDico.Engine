using Crolow.FastDico.Data.Bridge.Entities.ScrabbleApi;
using Kalow.Apps.Common.DataTypes;

namespace Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi
{
    public interface IGameConfigModel : IDataObject
    {
        KalowId BoardConfig { get; set; }
        KalowId LetterConfig { get; set; }
        KalowId Dictionary { get; set; }
        KalowId ReferenceDictionary { get; set; }
        int[] Bonus { get; }
        int CheckDistributionRound { get; set; }
        bool DifficultMode { get; set; }
        bool HelpAvailable { get; set; }
        int InRackLetters { get; set; }
        bool JokerMode { get; set; }

        string Name { get; set; }
        int PlayableLetters { get; set; }
        bool ShowDictionary { get; set; }
        int TimeByTurn { get; set; }
        bool ToppingMode { get; set; }

        IEvaluatorConfig BoostConfig { get; set; }
    }
}