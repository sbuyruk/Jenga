using Jenga.DataAccess.Repositories.IRepository;
using Jenga.DataAccess.Repositories.IRepository.Menu;
using Jenga.Models.Ortak;

namespace Jenga.DataAccess.Services.Menu
{
    public class PersonelMenuService : IPersonelMenuService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PersonelMenuService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<List<PersonelMenu>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.PersonelMenu.GetAllAsync(cancellationToken) as List<PersonelMenu> ?? new List<PersonelMenu>();

        public async Task<PersonelMenu?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.PersonelMenu.GetByIdAsync(id, cancellationToken);

        public async Task<IEnumerable<PersonelMenu>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
            => await _unitOfWork.PersonelMenu.GetByPersonelIdAsync(personelId, cancellationToken);

        public async Task<bool> AddAsync(PersonelMenu item, string? modifiedBy = null, CancellationToken cancellationToken = default)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            await _unitOfWork.PersonelMenu.AddAsync(item, cancellationToken);
            await _unitOfWork.PersonelMenu.SaveAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(PersonelMenu item, string? modifiedBy = null, CancellationToken cancellationToken = default)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            await _unitOfWork.PersonelMenu.UpdateAsync(item, modifiedBy, cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(PersonelMenu item, CancellationToken cancellationToken = default)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            _unitOfWork.PersonelMenu.Remove(item);
            return await Task.FromResult(true);
        }
    }
}