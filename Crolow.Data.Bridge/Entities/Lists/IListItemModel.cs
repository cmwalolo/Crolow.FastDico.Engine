using Crolow.TopMachine.Data.Bridge;
using Crolow.TopMachine.Data.Bridge.Enums;
using Kalow.Apps.Common.DataTypes;

namespace Crolow.TopMachine.Core.Entities.Lists
{
    public interface IListItemModel : IDataObject
    {
        KalowId ListId { get; set; }
        int FoundCount { get; set; }
        int FoundInRowCount { get; set; }
        int NotFoundCount { get; set; }
        string Rack { get; set; }
        List<string> Solutions { get; set; }
        StatusOfListItem Status { get; set; }
    }
}