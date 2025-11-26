using Crolow.TopMachine.Data.Bridge;
using Crolow.TopMachine.Data.Bridge.Enums;
using Kalow.Apps.Common.DataTypes;

namespace Crolow.TopMachine.Core.Entities.Lists
{
    public interface IListSolutionModel
    {
        int PrefixLength { get; set; }
        int SuffixLength { get; set; }
        string Prefix { get; set; }
        string Suffix { get; set; }
        string Solution { get; set; }
    }

    public interface IListItemModel : IDataObject
    {
        KalowId ListId { get; set; }
        int FoundCount { get; set; }
        int FoundInRowCount { get; set; }
        int NotFoundCount { get; set; }
        string Rack { get; set; }
        List<IListSolutionModel> Solutions { get; set; }

        StatusOfListItem Status { get; set; }
    }
}