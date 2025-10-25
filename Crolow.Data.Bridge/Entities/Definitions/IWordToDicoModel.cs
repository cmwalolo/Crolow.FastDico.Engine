using Crolow.TopMachine.Data.Bridge.Enums;
using Kalow.Apps.Common.DataTypes;

namespace Crolow.TopMachine.Data.Bridge.Entities.Definitions
{
    public interface IWordToDicoModel : IDataObject
    {
        KalowId Parent { get; set; }
        string Word { get; set; }
        WordType WordType { get; set; }
    }
}