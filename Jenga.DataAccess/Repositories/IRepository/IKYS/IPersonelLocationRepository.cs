using Jenga.Models.IKYS;
using Jenga.Models.Inventory;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Repositories.IRepository.IKYS
{
    public interface IPersonelLocationRepository : IRepository<PersonelLocation>
    {
        Task<List<PersonelLocation>> GetLocationsForPersonelAsync(int personelId, bool onlyActive = true, CancellationToken cancellationToken = default);
        Task<List<PersonelLocation>> GetPersonelsForLocationAsync(int locationId, bool onlyActive = true, CancellationToken cancellationToken = default);

        Task<PersonelLocation?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAssignmentAsync(PersonelLocation entity, string? createdBy = null, CancellationToken cancellationToken = default);
        Task<bool> RemoveAssignmentAsync(int personelId, int locationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Marks the given personel-location pair as primary for the person.
        /// Ensures at most one IsPrimary = true per person (transactional).
        /// If the target assignment doesn't exist, it will be created.
        /// </summary>
        Task<bool> SetPrimaryAsync(int personelId, int locationId, string? modifiedBy = null, CancellationToken cancellationToken = default);

        Task<bool> AnyAsync(Expression<Func<PersonelLocation, bool>> predicate, CancellationToken cancellationToken = default);
    }
}