using Jenga.DataAccess.Data;
using Jenga.Models.Common;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Services.Common
{
    public class RoleService : IRoleService
    {
        private const string Source = nameof(RoleService);

        private readonly ILogService _logService;
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly IDbContextScopeFactory _scopeFactory;

        public RoleService(
            ILogService logService,
            IDbContextFactory<ApplicationDbContext> dbFactory,
            IDbContextScopeFactory scopeFactory)
        {
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        }

        public async Task<Result<List<Role>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.Set<Role>().AsNoTracking().ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetAllAsync");
                return Result.Failure<List<Role>>(Error.Unexpected("Roller getirilemedi.", ex, "Role.GetAll.Failed"));
            }
        }

        public async Task<Result> AddAsync(Role role, CancellationToken cancellationToken = default)
        {
            if (role is null)
                return Result.Failure(Error.Validation("Role boş olamaz.", "Role.Null"));

            try
            {
                // 1) Role'u ekle ve identity'i alabilmek için ilk SaveChanges'i yap.

                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.Set<Role>().AddAsync(role, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.AddAsync");
                return Result.Failure(Error.Unexpected("Rol eklenemedi.", ex, "Role.Add.Failed"));
            }
        }

        public async Task<Result> UpdateAsync(Role role, CancellationToken cancellationToken = default)
        {
            if (role is null)
                return Result.Failure(Error.Validation("Role boş olamaz.", "Role.Null"));

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var trackedRole = await db.Set<Role>()
                    .FirstOrDefaultAsync(r => r.Id == role.Id, cancellationToken);

                if (trackedRole is null)
                    return Result.Failure(Error.NotFound($"G�ncellenecek rol bulunamadı (Id={role.Id}).", "Role.NotFound"));

                db.Entry(trackedRole).CurrentValues.SetValues(role);

                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.UpdateAsync");
                return Result.Failure(Error.Unexpected("Rol g�ncellenemedi.", ex, "Role.Update.Failed"));
            }
        }

        public async Task<Result<Role>> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var role = await db.Set<Role>()
                    .Include(r => r.PersonelRoles!)
                        .ThenInclude(pr => pr.Personel)
                    .Include(r => r.RoleMenus!)
                        .ThenInclude(rm => rm.Menu)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

                if (role is null)
                    return Result.Failure<Role>(Error.NotFound($"Rol bulunamadı (Id={id}).", "Role.NotFound"));

                return Result.Success(role);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetByIdWithRelationsAsync");
                return Result.Failure<Role>(Error.Unexpected("Rol getirilemedi.", ex, "Role.Get.Failed"));
            }
        }

        public async Task<Result> DeleteAsync(Role role, CancellationToken cancellationToken = default)
        {
            if (role is null)
                return Result.Failure(Error.Validation("Role boş olamaz.", "Role.Null"));

            // Tek context + tek transaction i�inde join + role silme.
            // Hata olursa using sonu rollback eder; "join'ler silindi ama role kaldi" durumu olusmaz.
            try
            {
                await using var scope = await _scopeFactory.CreateAsync(cancellationToken);
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
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.DeleteAsync");
                return Result.Failure(Error.Unexpected("Rol silinemedi.", ex, "Role.Delete.Failed"));
            }
        }

        public async Task<Result> AddWithRelationsAsync(Role role, CancellationToken cancellationToken = default)
        {
            if (role is null)
                return Result.Failure(Error.Validation("Role boş olamaz.", "Role.Null"));

            // Canary 3. tur: tek context + tek transaction.
            // Role + t�m join satirlari tek bir Commit i�inde persist edilir.
            // Iliski nesnelerini dogrudan context'e takmiyoruz; sadece FK alanlarini okuyup
            // YENI PersonelRole / RoleMenu nesneleri olusturuyoruz (graph traversal sorununa karsi).
            var personelRoles = role.PersonelRoles?.ToList();
            var roleMenus = role.RoleMenus?.ToList();
            // Role'un kendisini context'e eklerken nav koleksiyonlari g�rmesini istemiyoruz.
            role.PersonelRoles = null;
            role.RoleMenus = null;

            try
            {
                await using var scope = await _scopeFactory.CreateAsync(cancellationToken);
                var db = scope.Context;

                // 1) Role'u ekle ve identity'i alabilmek i�in ilk SaveChanges'i yap.
                await db.Set<Role>().AddAsync(role, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);

                // 2) Yeni PersonelRole satirlari
                if (personelRoles is { Count: > 0 })
                {
                    foreach (var pr in personelRoles)
                    {
                        await db.Set<PersonelRole>().AddAsync(new PersonelRole
                        {
                            RoleId = role.Id,
                            PersonelId = pr.PersonelId
                        }, cancellationToken);
                    }
                }

                // 3) Yeni RoleMenu satirlari
                if (roleMenus is { Count: > 0 })
                {
                    foreach (var rm in roleMenus)
                    {
                        await db.Set<RoleMenu>().AddAsync(new RoleMenu
                        {
                            RoleId = role.Id,
                            MenuId = rm.MenuId
                        }, cancellationToken);
                    }
                }

                await scope.CommitAsync(cancellationToken);

                // UI'da koleksiyonlar g�z�kmeye devam etsin diye geri yerlestir.
                role.PersonelRoles = personelRoles;
                role.RoleMenus = roleMenus;

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.AddWithRelationsAsync");
                // UI bozulmasin diye koleksiyonlari geri ver.
                role.PersonelRoles = personelRoles;
                role.RoleMenus = roleMenus;
                return Result.Failure(Error.Unexpected("Rol ve ilişkileri eklenemedi.", ex, "Role.AddWithRelations.Failed"));
            }
        }

        public async Task<Result> UpdateWithRelationsAsync(Role role, CancellationToken cancellationToken = default)
        {
            if (role is null)
                return Result.Failure(Error.Validation("Role boş olamaz.", "Role.Null"));

            // UI'dan gelen istenen "hedef" durum
            var desiredPersonelIds = role.PersonelRoles?
                .Select(pr => pr.PersonelId)
                .Distinct()
                .ToHashSet() ?? new HashSet<int>();

            var desiredMenuIds = role.RoleMenus?
                .Select(rm => rm.MenuId)
                .Distinct()
                .ToHashSet() ?? new HashSet<int>();

            try
            {
                await using var scope = await _scopeFactory.CreateAsync(cancellationToken);
                var db = scope.Context;

                // 1) Role skaler g�ncelleme
                var trackedRole = await db.Set<Role>()
                    .FirstOrDefaultAsync(r => r.Id == role.Id, cancellationToken);

                if (trackedRole is null)
                    return Result.Failure(Error.NotFound($"G�ncellenecek rol bulunamadı (Id={role.Id}).", "Role.NotFound"));

                db.Entry(trackedRole).CurrentValues.SetValues(role);

                // 2) PersonelRole diff
                var currentPRs = await db.Set<PersonelRole>()
                    .Where(pr => pr.RoleId == role.Id)
                    .ToListAsync(cancellationToken);

                var currentPersonelIds = currentPRs.Select(pr => pr.PersonelId).ToHashSet();

                var prsToRemove = currentPRs
                    .Where(pr => !desiredPersonelIds.Contains(pr.PersonelId))
                    .ToList();
                if (prsToRemove.Count > 0)
                    db.Set<PersonelRole>().RemoveRange(prsToRemove);

                foreach (var personelId in desiredPersonelIds)
                {
                    if (currentPersonelIds.Contains(personelId)) continue;

                    await db.Set<PersonelRole>().AddAsync(new PersonelRole
                    {
                        RoleId = role.Id,
                        PersonelId = personelId
                    }, cancellationToken);
                }

                // 3) RoleMenu diff
                var currentRMs = await db.Set<RoleMenu>()
                    .Where(rm => rm.RoleId == role.Id)
                    .ToListAsync(cancellationToken);

                var currentMenuIds = currentRMs.Select(rm => rm.MenuId).ToHashSet();

                var rmsToRemove = currentRMs
                    .Where(rm => !desiredMenuIds.Contains(rm.MenuId))
                    .ToList();
                if (rmsToRemove.Count > 0)
                    db.Set<RoleMenu>().RemoveRange(rmsToRemove);

                foreach (var menuId in desiredMenuIds)
                {
                    if (currentMenuIds.Contains(menuId)) continue;

                    await db.Set<RoleMenu>().AddAsync(new RoleMenu
                    {
                        RoleId = role.Id,
                        MenuId = menuId
                    }, cancellationToken);
                }

                await scope.CommitAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.UpdateWithRelationsAsync");
                return Result.Failure(Error.Unexpected("Rol ve ilişkileri g�ncellenemedi.", ex, "Role.UpdateWithRelations.Failed"));
            }
        }
    }
}
