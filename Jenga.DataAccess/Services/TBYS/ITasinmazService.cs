using Jenga.Models.TBYS;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public interface ITasinmazService
    {
        Task<List<Tasinmaz>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<List<Tasinmaz>> GetByEnvanterDurumuAsync(int envanterdeMi, CancellationToken cancellationToken = default);
        Task<List<Tasinmaz>> GetEnvanterdekilerAsync(CancellationToken cancellationToken = default);

        Task<Tasinmaz?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Tasinmaz?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(Tasinmaz tasinmaz, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(Tasinmaz tasinmaz, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int tasinmazId, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<Tasinmaz, bool>> predicate, CancellationToken cancellationToken = default);
        Task<(bool CanDelete, string? Reason)> CanDeleteAsync(int id);
        Task<bool> ExistsByEmlakSicilNoAsync(string emlakSicilNo, int? excludeId = null, CancellationToken cancellationToken = default);
    }
}
