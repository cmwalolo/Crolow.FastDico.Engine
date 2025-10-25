using Crolow.TopMachine.Data.Bridge.DataObjects;
using Crolow.TopMachine.Data.Bridge.Entities.Definitions;
using Crolow.TopMachine.Data.Bridge.Enums;
using Kalow.Apps.Common.DataTypes;

namespace Crolow.TopMachine.Core.Entities.Definitions
{
    public class WordToDicoModel : DataObject, IWordToDicoModel
    {
        public string Word { get; set; }
        public KalowId Parent { get; set; } = KalowId.Empty;
        public WordType WordType { get; set; } = WordType.Unknown;
    }
}
