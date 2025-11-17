using Jenga.DataAccess.Repositories.IRepository;
using Jenga.DataAccess.Repositories.IRepository.IKYS;
using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS
{


    public class PersonelLocationService : IPersonelLocationService
    {
        private readonly IUnitOfWork _uow;

        public PersonelLocationService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<List<PersonelLocation>> GetLocationsForPersonelAsync(int personelId, bool onlyActive = true, CancellationToken cancellationToken = default)
        {
            return await _uow.PersonelLocation.GetLocationsForPersonelAsync(personelId, onlyActive, cancellationToken);
        }

        public async Task<List<PersonelLocation>> GetPersonelsForLocationAsync(int locationId, bool onlyActive = true, CancellationToken cancellationToken = default)
        {
            return await _uow.PersonelLocation.GetPersonelsForLocationAsync(locationId, onlyActive, cancellationToken);
        }

        public async Task<PersonelLocation?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _uow.PersonelLocation.GetByIdAsync(id, cancellationToken);
        }

        public async Task<bool> AssignLocationToPersonAsync(PersonelLocation assignment, string? createdBy = null, CancellationToken cancellationToken = default)
        {
            if (assignment == null) return false;
            return await _uow.PersonelLocation.AddAssignmentAsync(assignment, createdBy, cancellationToken);
        }

        public async Task<bool> UnassignLocationFromPersonAsync(int personelId, int locationId, CancellationToken cancellationToken = default)
        {
            return await _uow.PersonelLocation.RemoveAssignmentAsync(personelId, locationId, cancellationToken);
        }

        public async Task<bool> SetPrimaryAssignmentAsync(int personelId, int locationId, string? modifiedBy = null, CancellationToken cancellationToken = default)
        {
            return await _uow.PersonelLocation.SetPrimaryAsync(personelId, locationId, modifiedBy, cancellationToken);
        }
    }
}