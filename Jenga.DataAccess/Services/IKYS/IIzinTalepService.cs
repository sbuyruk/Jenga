using Jenga.Models.IKYS;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.IKYS;

public interface IIzinTalepService
{
    Task<Result<List<IzinTalep>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<List<IzinTalep>>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default);
    Task<Result<IzinTalep>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> AddAsync(IzinTalep entity, string? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(IzinTalep entity, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(IzinTalep entity, CancellationToken cancellationToken = default);
}
