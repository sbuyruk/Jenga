using Jenga.Models.Ortak;

namespace Jenga.DataAccess.Services.Menu
{
    public interface IPersonelMenuService
    {
        Task<List<PersonelMenu>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PersonelMenu?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(PersonelMenu item, string? modifiedBy = null, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(PersonelMenu item, string? modifiedBy = null, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(PersonelMenu item, CancellationToken cancellationToken = default);
        Task<IEnumerable<PersonelMenu>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default);
    }
}