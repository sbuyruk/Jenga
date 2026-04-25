using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public interface IAileService
{
    Task<List<Aile>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Aile>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default);
    Task<Aile?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> AddAsync(Aile aile, string? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Aile aile, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Aile aile, CancellationToken cancellationToken = default);
}
