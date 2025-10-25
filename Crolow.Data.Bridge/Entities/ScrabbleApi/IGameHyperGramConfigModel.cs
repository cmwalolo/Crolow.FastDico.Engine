using Kalow.Apps.Common.DataTypes;

namespace Crolow.TopMachine.Data.Bridge.Entities.ScrabbleApi
{
    public interface IGameHypergramConfigModel : IDataObject
    {
        KalowId LetterConfig { get; set; }
        KalowId Dictionary { get; set; }
        KalowId ReferenceDictionary { get; set; }
        int InRackLetters { get; set; }
        bool JokerMode { get; set; }

        string Name { get; set; }
        int TimeByTurn { get; set; }
        string LettersOnRack { get; set; }
    }
}