using Jenga.Models.Inventory;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Repositories.IRepository
{
    public interface IRepository<T> where T : class
    {


        List<T> GetByFilter(Expression<Func<T, bool>> filter, string? includeProperties = null);
        T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null);
        IEnumerable<T> GetAll(string? includeProperties = null);
        void Add(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entity);
        void Update(T entity); //SB
        
        //async interfaces //SB
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

        Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter, string? includeProperties = null, bool trackChanges = true);
        Task<IEnumerable<T>> GetAllAsync(string? includeProperties = null);
        Task<IEnumerable<T>> GetAllByFilterAsync(Expression<Func<T, bool>> filter, string? includeProperties = null);

    }
}
