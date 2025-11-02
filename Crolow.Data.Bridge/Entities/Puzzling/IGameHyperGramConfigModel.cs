using Crolow.TopMachine.Data.Bridge;
using Kalow.Apps.Common.DataTypes;

namespace Crolow.FastDico.Data.Bridge.Entities.Puzzling
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