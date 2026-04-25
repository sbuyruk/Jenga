using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public interface IIzinDonemService
{
    Task<List<IzinDonem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<IzinDonem>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default);
    Task<IzinDonem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> AddAsync(IzinDonem entity, string? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(IzinDonem entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(IzinDonem entity, CancellationToken cancellationToken = default);
}
