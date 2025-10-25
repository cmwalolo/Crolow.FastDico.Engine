using Crolow.TopMachine.Data.Bridge.Entities.Lists;
using Crolow.TopMachine.Data.Bridge.Enums;

namespace Crolow.TopMachine.Core.Entities.Lists
{
    public class ListCriteria : IListCriteria
    {
        public string InclusionParameter { get; set; } = string.Empty;
        public TypeOfCriteria InclusionCriteria { get; set; } = TypeOfCriteria.None;
        public int InclusionLength { get; set; } = 1;
        public string ExclusionParameter { get; set; } = string.Empty;
        public TypeOfCriteria ExclusionCriteria { get; set; } = TypeOfCriteria.None;
        public int ExclusionLength { get; set; } = 1;

    }
}