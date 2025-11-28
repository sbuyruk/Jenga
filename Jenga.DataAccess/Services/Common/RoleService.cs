using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Common;
using Jenga.Utility.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Services.Common
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogService _logService;

        public RoleService(IUnitOfWork unitOfWork, ILogService logService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public async Task<List<Role>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.Role.GetAllAsync(cancellationToken);
        }

        public async Task<bool> AddAsync(Role role, CancellationToken cancellationToken = default)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));

            role.Olusturan ??= Environment.UserName;
            role.OlusturmaTarihi ??= DateTime.Now;

            // Add role (repository AddAsync commits)
            await _unitOfWork.Role.AddAsync(role, cancellationToken);

            return true;
        }

        public async Task<bool> UpdateAsync(Role role, CancellationToken cancellationToken = default)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));

            role.Degistiren = Environment.UserName;
            role.DegistirmeTarihi = DateTime.Now;

            await _unitOfWork.Role.UpdateAsync(role, null, cancellationToken);
            return true;
        }

        public async Task<Role?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default)
        {
            // Delegate to repository which already includes navigation properties
            return await _unitOfWork.Role.GetByIdWithRelationsAsync(id, cancellationToken);
        }

        public async Task<bool> DeleteAsync(Role role, CancellationToken cancellationToken = default)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));

            try
            {
                // Remove join rows first to avoid FK constraint issues
                var existingPRs = (await _unitOfWork.PersonelRole.GetAllByFilterAsync(pr => pr.RoleId == role.Id)).ToList();
                if (existingPRs.Any())
                {
                    _unitOfWork.PersonelRole.RemoveRange(existingPRs);
                }

                var existingRMs = (await _unitOfWork.RoleMenu.GetAllByFilterAsync(rm => rm.RoleId == role.Id)).ToList();
                if (existingRMs.Any())
                {
                    _unitOfWork.RoleMenu.RemoveRange(existingRMs);
                }

                // Remove role
                _unitOfWork.Role.Remove(role);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("RoleService.DeleteAsync error", ex);
                throw;
            }
        }

        public async Task<bool> AddWithRelationsAsync(Role role, CancellationToken cancellationToken = default)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));

            role.Olusturan ??= Environment.UserName;
            role.OlusturmaTarihi ??= DateTime.Now;

            // Persist role first (will set role.Id)
            await _unitOfWork.Role.AddAsync(role, cancellationToken);

            // Persist PersonelRole join rows (only FKs, navigation props nulled)
            if (role.PersonelRoles != null && role.PersonelRoles.Any())
            {
                foreach (var pr in role.PersonelRoles)
                {
                    pr.Id = 0; // ensure EF will generate identity (avoid explicit identity insert)
                    pr.RoleId = role.Id;
                    pr.Personel = null;
                    pr.Role = null;
                    pr.Olusturan ??= Environment.UserName;
                    pr.OlusturmaTarihi ??= DateTime.Now;
                    await _unitOfWork.PersonelRole.AddAsync(pr, cancellationToken);
                }
            }

            // Persist RoleMenu join rows
            if (role.RoleMenus != null && role.RoleMenus.Any())
            {
                foreach (var rm in role.RoleMenus)
                {
                    rm.Id = 0; // ensure EF will generate identity
                    rm.RoleId = role.Id;
                    rm.Menu = null;
                    rm.Role = null;
                    rm.Olusturan ??= Environment.UserName;
                    rm.OlusturmaTarihi ??= DateTime.Now;
                    await _unitOfWork.RoleMenu.AddAsync(rm, cancellationToken);
                }
            }

            return true;
        }

        public async Task<bool> UpdateWithRelationsAsync(Role role, CancellationToken cancellationToken = default)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));

            // Update role scalars
            role.Degistiren = Environment.UserName;
            role.DegistirmeTarihi = DateTime.Now;
            await _unitOfWork.Role.UpdateAsync(role, null, cancellationToken);

            // Replace PersonelRole entries
            var existingPRs = (await _unitOfWork.PersonelRole.GetAllByFilterAsync(pr => pr.RoleId == role.Id)).ToList();
            if (existingPRs.Any())
                _unitOfWork.PersonelRole.RemoveRange(existingPRs);

            if (role.PersonelRoles != null && role.PersonelRoles.Any())
            {
                foreach (var pr in role.PersonelRoles)
                {
                    pr.Id = 0; // reset Id so EF treats this as new identity row
                    pr.RoleId = role.Id;
                    pr.Personel = null;
                    pr.Role = null;
                    pr.Olusturan ??= Environment.UserName;
                    pr.OlusturmaTarihi ??= DateTime.Now;
                    await _unitOfWork.PersonelRole.AddAsync(pr, cancellationToken);
                }
            }

            // Replace RoleMenu entries
            var existingRMs = (await _unitOfWork.RoleMenu.GetAllByFilterAsync(rm => rm.RoleId == role.Id)).ToList();
            if (existingRMs.Any())
                _unitOfWork.RoleMenu.RemoveRange(existingRMs);

            if (role.RoleMenus != null && role.RoleMenus.Any())
            {
                foreach (var rm in role.RoleMenus)
                {
                    rm.Id = 0; // reset Id so EF treats this as new identity row
                    rm.RoleId = role.Id;
                    rm.Menu = null;
                    rm.Role = null;
                    rm.Olusturan ??= Environment.UserName;
                    rm.OlusturmaTarihi ??= DateTime.Now;
                    await _unitOfWork.RoleMenu.AddAsync(rm, cancellationToken);
                }
            }

            return true;
        }
    }
}
