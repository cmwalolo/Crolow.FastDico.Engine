using Kalow.Apps.Common.DataTypes;
using System.Linq.Expressions;

namespace Crolow.TopMachine.Data.Bridge
{
    public interface IDataManager<T> : IDisposable where T : IDataObject
    {
        IRepository Repository { get; set; }
        IDocumentSession DocumentSession { get; set; }
        Task<IEnumerable<T>> GetAllNodes();
        Task<IEnumerable<T>> GetAllNodes(Expression<Func<T, bool>> filter);
        Task<T> GetNode(Expression<Func<T, bool>> filter);
        Task<T> GetNode(KalowId dataLink);
        Task Update(T data);
        Task UpdateAll(IEnumerable<T> data);
        IDocumentSession StartSession();
    }
}