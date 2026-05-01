using Jenga.Models.IKYS;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.IKYS;

public interface IIzinHareketService
{
    Task<Result<List<IzinHareket>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<List<IzinHareket>>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default);
    Task<Result<IzinHareket>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> AddAsync(IzinHareket entity, string? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(IzinHareket entity, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(IzinHareket entity, CancellationToken cancellationToken = default);
}
