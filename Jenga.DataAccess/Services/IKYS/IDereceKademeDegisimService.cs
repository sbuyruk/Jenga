using Jenga.Models.IKYS;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.IKYS;

public interface IDereceKademeDegisimService
{
    Task<Result<List<DereceKademeDegisim>>> GetDereceYukseltmeAsync(CancellationToken cancellationToken = default);
    Task<Result<List<DereceKademeDegisim>>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default);
    Task<Result<DereceKademeDegisim>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> AddAsync(DereceKademeDegisim entity, string? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(DereceKademeDegisim entity, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(DereceKademeDegisim entity, CancellationToken cancellationToken = default);
    Task<Result<List<DereceKademeDegisim>>> GetAllAsync(CancellationToken cancellationToken = default);
}
