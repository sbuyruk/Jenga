using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public interface IIzinHareketService
{
    Task<List<IzinHareket>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<IzinHareket>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default);
    Task<IzinHareket?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> AddAsync(IzinHareket entity, string? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(IzinHareket entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(IzinHareket entity, CancellationToken cancellationToken = default);
}
