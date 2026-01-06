using Jenga.Models.TBYS;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public interface ITasinmazBagisciService
    {
        Task<List<TasinmazBagisci>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<TasinmazBagisci?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<TasinmazBagisci?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(TasinmazBagisci bagisci, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(TasinmazBagisci bagisci, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int bagisciId, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<TasinmazBagisci, bool>> predicate, CancellationToken cancellationToken = default);
        Task<(bool CanDelete, string? Reason)> CanDeleteAsync(int id);
        Task<bool> ExistsByTCKimlikAsync(long? tckimlik, int? excludeId = null, CancellationToken cancellationToken = default);
    }
}