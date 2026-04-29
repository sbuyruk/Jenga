using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Common;
using Jenga.Utility.Logging;
using Microsoft.EntityFrameworkCore;
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
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public RoleService(
            IUnitOfWork unitOfWork,
            ILogService logService,
            IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
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

            // Canary: tek context + tek transaction içinde join + role silme.
            // Hata olursa using sonu rollback eder; "join'ler silindi ama role kaldı" durumu oluşmaz.
            try
            {
                await using var scope = await DbContextScope.CreateAsync(_dbFactory, cancellationToken);
                var db = scope.Context;

                var existingPRs = await db.Set<PersonelRole>()
                    .Where(pr => pr.RoleId == role.Id)
                    .ToListAsync(cancellationToken);
                if (existingPRs.Count > 0)
                    db.Set<PersonelRole>().RemoveRange(existingPRs);

                var existingRMs = await db.Set<RoleMenu>()
                    .Where(rm => rm.RoleId == role.Id)
                    .ToListAsync(cancellationToken);
                if (existingRMs.Count > 0)
                    db.Set<RoleMenu>().RemoveRange(existingRMs);

                var roleEntity = await db.Set<Role>()
                    .FirstOrDefaultAsync(r => r.Id == role.Id, cancellationToken);
                if (roleEntity != null)
                    db.Set<Role>().Remove(roleEntity);

                await scope.CommitAsync(cancellationToken);
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

            // Minimal patch: ilişki koleksiyonlarını role nesnesinden detach et.
            // Aksi halde EF, AddAsync sırasında graph traversal yaparak Personel ve Menu
            // kayıtlarını da "Added" olarak işaretliyor → IDENTITY_INSERT OFF hatası.
            // Join satırlarını aşağıda zaten tek tek ekliyoruz.
            var personelRoles = role.PersonelRoles?.ToList();
            var roleMenus = role.RoleMenus?.ToList();
            role.PersonelRoles = null;
            role.RoleMenus = null;

            // Persist role first (will set role.Id)
            await _unitOfWork.Role.AddAsync(role, cancellationToken);

            // Persist PersonelRole join rows (only FKs, navigation props nulled)
            if (personelRoles != null && personelRoles.Count > 0)
            {
                foreach (var pr in personelRoles)
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
            if (roleMenus != null && roleMenus.Count > 0)
            {
                foreach (var rm in roleMenus)
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

            // Çağıranın UI'da listeyi göstermeye devam edebilmesi için koleksiyonları geri yerleştir.
            role.PersonelRoles = personelRoles;
            role.RoleMenus = roleMenus;

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
