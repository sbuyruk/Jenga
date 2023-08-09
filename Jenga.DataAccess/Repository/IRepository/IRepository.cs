using System.Linq.Expressions;

namespace Jenga.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        T GetFirstOrDefault(Expression<Func<T,bool>> filter, string? includeProperties = null);
        List<T> GetByFilter(Expression<Func<T,bool>> filter, string? includeProperties = null);
        IEnumerable<T> GetAll(string? includeProperties = null);
        void Add(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entity);
        Task<IEnumerable<T>> GetAllAsync();
    }
}
