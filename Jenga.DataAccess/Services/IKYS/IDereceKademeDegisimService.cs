using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public interface IDereceKademeDegisimService
{
    Task<List<DereceKademeDegisim>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<DereceKademeDegisim>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default);
    Task<DereceKademeDegisim?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> AddAsync(DereceKademeDegisim entity, string? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(DereceKademeDegisim entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(DereceKademeDegisim entity, CancellationToken cancellationToken = default);
}
