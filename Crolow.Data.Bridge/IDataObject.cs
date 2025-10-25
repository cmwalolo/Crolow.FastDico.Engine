using Kalow.Apps.Common.DataTypes;

namespace Crolow.TopMachine.Data.Bridge
{
    public enum EditState
    {
        Unchanged = 0,
        New = 1,
        Update = 2,
        ToDelete = 4,
        Deleted = 8

    }
    public interface IDataObject
    {
        EditState EditState { get; set; }
        KalowId Id { get; set; }
    }
}