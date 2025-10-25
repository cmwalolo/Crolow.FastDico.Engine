using Crolow.TopMachine.Data.Bridge;
using Kalow.Apps.Common.DataTypes;

namespace Crolow.FastDico.Common.Models.ScrabbleApi.Entities
{
    public interface IGameRollerConfigModel : IDataObject
    {
        KalowId GameConfigId { get; set; }
        string LettersOnRack { get; set; }
        string MandatoryLettersOnRack { get; set; }
        string Name { get; set; }
        int NumberOfRounds { get; set; }
    }
}