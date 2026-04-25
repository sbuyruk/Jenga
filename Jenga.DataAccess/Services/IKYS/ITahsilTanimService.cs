using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public interface ITahsilTanimService
{
    Task<List<TahsilTanim>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TahsilTanim?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> AddAsync(TahsilTanim entity, string? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(TahsilTanim entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(TahsilTanim entity, CancellationToken cancellationToken = default);
}
