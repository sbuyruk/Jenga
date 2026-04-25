using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public interface IBirimTanimService
{
    Task<List<BirimTanim>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<BirimTanim?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> AddAsync(BirimTanim entity, string? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(BirimTanim entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(BirimTanim entity, CancellationToken cancellationToken = default);
}
