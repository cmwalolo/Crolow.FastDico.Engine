using Crolow.TopMachine.Data.Bridge.Enums;

namespace Crolow.TopMachine.Data.Bridge.Entities.Lists
{
    public interface IListCriteria
    {
        TypeOfCriteria ExclusionCriteria { get; set; }
        int ExclusionLength { get; set; }
        string ExclusionParameter { get; set; }
        TypeOfCriteria InclusionCriteria { get; set; }
        int InclusionLength { get; set; }
        string InclusionParameter { get; set; }
    }
}