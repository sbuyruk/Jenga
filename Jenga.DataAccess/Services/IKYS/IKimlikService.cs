using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS;

public interface IKimlikService
{
    Task<List<Kimlik>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Kimlik?> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default);
    Task<Kimlik?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> AddAsync(Kimlik entity, string? modifiedBy = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Kimlik entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Kimlik entity, CancellationToken cancellationToken = default);
}
