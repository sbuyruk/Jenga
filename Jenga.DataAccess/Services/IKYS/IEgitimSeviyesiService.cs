using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public interface IEgitimSeviyesiService
{
    Task<List<EgitimSeviyesi>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<EgitimSeviyesi?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> AddAsync(EgitimSeviyesi entity, string? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(EgitimSeviyesi entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(EgitimSeviyesi entity, CancellationToken cancellationToken = default);
}
