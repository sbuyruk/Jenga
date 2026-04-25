using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public interface IIzinTalepService
{
    Task<List<IzinTalep>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<IzinTalep>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default);
    Task<IzinTalep?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> AddAsync(IzinTalep entity, string? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(IzinTalep entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(IzinTalep entity, CancellationToken cancellationToken = default);
}
