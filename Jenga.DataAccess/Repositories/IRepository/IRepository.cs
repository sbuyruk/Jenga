using System.Linq.Expressions;

namespace Jenga.DataAccess.Repositories.IRepository
{
    public interface IRepository<T> where T : Jenga.Models.Sistem.BaseModel
    {
        // Remove single entity (does not SaveChanges by itself)
        void Remove(T entity);

        // Remove multiple entities (does not SaveChanges by itself)
        void RemoveRange(IEnumerable<T> entities);

        // Synchronous update (does not SaveChanges by itself)
        void Update(T entity);

        // Async update and commit
        Task UpdateAsync(T entity, string? modifiedBy = null, CancellationToken cancellationToken = default);

        // Add entity (caller decides whether repository commits or not)
        Task AddAsync(T entity, CancellationToken cancellationToken = default);

        // Persist changes (if repository/operation uses separate DbContext instance).
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        // Query helpers
        Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default);

        // Get with include properties (comma separated)
        Task<IEnumerable<T>> GetAllAsync(string? includeProperties = null);

        Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter, string? includeProperties = null, bool trackChanges = true);

        Task<IEnumerable<T>> GetAllByFilterAsync(Expression<Func<T, bool>> filter, string? includeProperties = null);

        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    }
}