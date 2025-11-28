using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Common;
using Jenga.Utility.Logging;

namespace Jenga.DataAccess.Services.Menu
{
    public class PersonelRoleService : IPersonelRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogService _logService;

        public PersonelRoleService(IUnitOfWork unitOfWork, ILogService logService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public async Task<List<PersonelRole>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var items = await _unitOfWork.PersonelRole.GetAllAsync(cancellationToken);
                return items.ToList();
            }
            catch (Exception ex)
            {
                _logService.LogError("PersonelRoleService.GetAllAsync", ex);
                return new List<PersonelRole>();
            }
        }

        public async Task<PersonelRole?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.PersonelRole.GetByIdAsync(id, cancellationToken);

        public async Task<IEnumerable<PersonelRole>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
            => await _unitOfWork.PersonelRole.GetByPersonelIdAsync(personelId, cancellationToken);

        public async Task<bool> AddAsync(PersonelRole personelRole, CancellationToken cancellationToken = default)
        {
            if (personelRole == null) throw new ArgumentNullException(nameof(personelRole));

            try
            {
                await _unitOfWork.PersonelRole.AddAsync(personelRole, cancellationToken);
                await _unitOfWork.PersonelRole.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService.LogError("PersonelRoleService.AddAsync", ex);
                return false;
            }
        }

        public async Task<bool> UpdateAsync(PersonelRole personelRole, CancellationToken cancellationToken = default)
        {
            if (personelRole == null) throw new ArgumentNullException(nameof(personelRole));

            try
            {
                await _unitOfWork.PersonelRole.UpdateAsync(personelRole);
                await _unitOfWork.PersonelRole.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService.LogError("PersonelRoleService.UpdateAsync", ex);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(PersonelRole personelRole, CancellationToken cancellationToken = default)
        {
            if (personelRole == null) return false;

            try
            {
                _unitOfWork.PersonelRole.Remove(personelRole);
                await _unitOfWork.PersonelRole.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService.LogError("PersonelRoleService.DeleteAsync", ex);
                return false;
            }
        }
    }
}
