
namespace Crolow.TopMachine.Core.Entities.Lists
{
    public interface IListWordModel
    {
        string Rack { get; set; }
        List<IListSolutionModel> Solutions { get; set; }
    }
}