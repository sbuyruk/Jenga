using Jenga.Models.IKYS;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.IKYS
{
    public interface IPersonelLocationService
    {
        Task<Result<List<PersonelLocation>>> GetLocationsForPersonelAsync(int personelId, bool onlyActive = true, CancellationToken cancellationToken = default);
        Task<Result<List<PersonelLocation>>> GetPersonelsForLocationAsync(int locationId, bool onlyActive = true, CancellationToken cancellationToken = default);

        Task<Result<PersonelLocation>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> AssignLocationToPersonAsync(PersonelLocation assignment, string? createdBy = null, CancellationToken cancellationToken = default);
        Task<Result> UnassignLocationFromPersonAsync(int personelId, int locationId, CancellationToken cancellationToken = default);

        Task<Result> SetPrimaryAssignmentAsync(int personelId, int locationId, string? modifiedBy = null, CancellationToken cancellationToken = default);
    }
}