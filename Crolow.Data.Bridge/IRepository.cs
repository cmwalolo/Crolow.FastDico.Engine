using Kalow.Apps.Common.DataTypes;
using System.Linq.Expressions;

namespace Crolow.TopMachine.Data.Bridge
{
    public interface IRepository : IDisposable
    {
        #region Utils
        void CreateIndex<T>(string name, string fields);
        void DropIndex<T>(string name);
        #endregion

        #region Reads
        Task<T> Get<T>(Expression<Func<T, bool>> filter);
        Task<int> Count<T>(Expression<Func<T, bool>> filter);
        Task<IEnumerable<T>> GetAll<T>();
        Task<IEnumerable<T>> List<T>(Expression<Func<T, bool>> filter);
        Task<IEnumerable<T>> List<T>(
            Expression<Func<T, bool>> filter,
            string order = null,
            bool isDescending = false,
            int skip = 0,
            int take = int.MaxValue);

        Task<IEnumerable<IGrouping<TKey, T>>> GroupAsync<TKey, T>(Expression<Func<T, bool>> filter, Expression<Func<T, TKey>> group);

        Task<Dictionary<TKey, int>> CountByGroupAsync<T, TKey>(Expression<Func<T, bool>> filter,
                                                        Expression<Func<T, TKey>> group);

        #endregion

        #region Writes
        Task Add<T>(T item);
        Task AddBulk<T>(IEnumerable<T> items);
        Task<bool> Remove<T>(Expression<Func<T, bool>> filter);
        Task<bool> Remove<T>(KalowId id);
        Task<bool> Update<T>(Expression<Func<T, bool>> filter, T item);
        #endregion

        #region Transactions
        IDocumentSession StartSession();
        #endregion
    }
}