using Jenga.Models.IKYS;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.IKYS;

public interface IIzinDonemService
{
    Task<Result<List<IzinDonem>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<List<IzinDonem>>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default);
    Task<Result<IzinDonem>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> AddAsync(IzinDonem entity, string? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(IzinDonem entity, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(IzinDonem entity, CancellationToken cancellationToken = default);
}
