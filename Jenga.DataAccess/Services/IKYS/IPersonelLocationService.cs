using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS
{
    public interface IPersonelLocationService
    {
        Task<List<PersonelLocation>> GetLocationsForPersonelAsync(int personelId, bool onlyActive = true, CancellationToken cancellationToken = default);
        Task<List<PersonelLocation>> GetPersonelsForLocationAsync(int locationId, bool onlyActive = true, CancellationToken cancellationToken = default);

        Task<PersonelLocation?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AssignLocationToPersonAsync(PersonelLocation assignment, string? createdBy = null, CancellationToken cancellationToken = default);
        Task<bool> UnassignLocationFromPersonAsync(int personelId, int locationId, CancellationToken cancellationToken = default);

        Task<bool> SetPrimaryAssignmentAsync(int personelId, int locationId, string? modifiedBy = null, CancellationToken cancellationToken = default);
    }
}