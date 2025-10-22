using Jenga.Models.Inventory;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public interface IMaterialExitService
    {
        Task<List<MaterialExit>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<MaterialExit?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task AddAsync(MaterialExit exit, CancellationToken cancellationToken = default);
        Task UpdateAsync(MaterialExit exit, CancellationToken cancellationToken = default);
        Task DeleteAsync(MaterialExit exit, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<MaterialExit, bool>> predicate);

        // Ekstra filtreleme/sorgu fonksiyonları da eklenebilir
    }
}