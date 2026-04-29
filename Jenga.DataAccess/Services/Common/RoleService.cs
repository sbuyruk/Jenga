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

            // Canary 3. tur: tek context + tek transaction.
            // Role + tüm join satırları tek bir Commit içinde persist edilir.
            // İlişki nesnelerini doğrudan context'e takmıyoruz; sadece FK alanlarını okuyup
            // YENİ PersonelRole / RoleMenu nesneleri oluşturuyoruz (graph traversal sorununa karşı).
            var personelRoles = role.PersonelRoles?.ToList();
            var roleMenus = role.RoleMenus?.ToList();
            // Role'un kendisini context'e eklerken nav koleksiyonları görmesini istemiyoruz.
            role.PersonelRoles = null;
            role.RoleMenus = null;

            try
            {
                await using var scope = await DbContextScope.CreateAsync(_dbFactory, cancellationToken);
                var db = scope.Context;

                // 1) Role'u ekle ve identity'i alabilmek için ilk SaveChanges'i yap.
                //    Hâlâ aynı transaction içindeyiz; commit sadece scope.CommitAsync'te olur.
                await db.Set<Role>().AddAsync(role, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                // Bu noktada role.Id atanmıştır.

                // 2) Yeni PersonelRole satırlarını ekle (FK'leri kullanarak yeni nesneler)
                if (personelRoles != null && personelRoles.Count > 0)
                {
                    foreach (var pr in personelRoles)
                    {
                        var newPr = new PersonelRole
                        {
                            RoleId = role.Id,
                            PersonelId = pr.PersonelId,
                            Olusturan = pr.Olusturan ?? Environment.UserName,
                            OlusturmaTarihi = pr.OlusturmaTarihi ?? DateTime.Now
                        };
                        await db.Set<PersonelRole>().AddAsync(newPr, cancellationToken);
                    }
                }

                // 3) Yeni RoleMenu satırlarını ekle
                if (roleMenus != null && roleMenus.Count > 0)
                {
                    foreach (var rm in roleMenus)
                    {
                        var newRm = new RoleMenu
                        {
                            RoleId = role.Id,
                            MenuId = rm.MenuId,
                            Olusturan = rm.Olusturan ?? Environment.UserName,
                            OlusturmaTarihi = rm.OlusturmaTarihi ?? DateTime.Now
                        };
                        await db.Set<RoleMenu>().AddAsync(newRm, cancellationToken);
                    }
                }

                // 4) Join satırları için ikinci SaveChanges + transaction Commit.
                await scope.CommitAsync(cancellationToken);

                // UI'da koleksiyonlar gözükmeye devam etsin diye geri yerleştir.
                role.PersonelRoles = personelRoles;
                role.RoleMenus = roleMenus;

                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("RoleService.AddWithRelationsAsync error", ex);
                // Çağırana koleksiyonları geri vermek için (UI bozulmasın diye)
                role.PersonelRoles = personelRoles;
                role.RoleMenus = roleMenus;
                throw;
            }
        }

        public async Task<bool> UpdateWithRelationsAsync(Role role, CancellationToken cancellationToken = default)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));

            // Canary 2. tur: tek context + tek transaction.
            // Eski join satırlarını sil + yeni join satırlarını ekle + role skaler güncelle
            // → hepsi tek SaveChanges + Commit içinde. Hata olursa rollback.
            //
            // Önemli: Role nesnesi UI'dan PersonelRoles / RoleMenus dolu (içlerinde Personel/Menu nav prop'ları
            // dolu) olarak gelebilir. EF graph traversal nedeniyle bu nesneleri doğrudan context'e takmıyoruz;
            // sadece skaler alanları kullanarak yeni satırlar oluşturuyoruz.
            var personelRoles = role.PersonelRoles?.ToList();
            var roleMenus = role.RoleMenus?.ToList();

            try
            {
                await using var scope = await DbContextScope.CreateAsync(_dbFactory, cancellationToken);
                var db = scope.Context;

                // 1) Role skaler güncelleme (mevcut entity'i çek, değerleri kopyala)
                var trackedRole = await db.Set<Role>()
                    .FirstOrDefaultAsync(r => r.Id == role.Id, cancellationToken);
                if (trackedRole == null)
                    throw new InvalidOperationException($"Güncellenecek Role bulunamadı (Id={role.Id}).");

                db.Entry(trackedRole).CurrentValues.SetValues(role);
                trackedRole.Degistiren = Environment.UserName;
                trackedRole.DegistirmeTarihi = DateTime.Now;

                // 2) Eski PersonelRole satırlarını sil
                var existingPRs = await db.Set<PersonelRole>()
                    .Where(pr => pr.RoleId == role.Id)
                    .ToListAsync(cancellationToken);
                if (existingPRs.Count > 0)
                    db.Set<PersonelRole>().RemoveRange(existingPRs);

                // 3) Eski RoleMenu satırlarını sil
                var existingRMs = await db.Set<RoleMenu>()
                    .Where(rm => rm.RoleId == role.Id)
                    .ToListAsync(cancellationToken);
                if (existingRMs.Count > 0)
                    db.Set<RoleMenu>().RemoveRange(existingRMs);

                // 4) Yeni PersonelRole satırlarını ekle (sadece FK, nav prop'lar null)
                if (personelRoles != null && personelRoles.Count > 0)
                {
                    foreach (var pr in personelRoles)
                    {
                        var newPr = new PersonelRole
                        {
                            RoleId = role.Id,
                            PersonelId = pr.PersonelId,
                            Olusturan = pr.Olusturan ?? Environment.UserName,
                            OlusturmaTarihi = pr.OlusturmaTarihi ?? DateTime.Now
                        };
                        await db.Set<PersonelRole>().AddAsync(newPr, cancellationToken);
                    }
                }

                // 5) Yeni RoleMenu satırlarını ekle
                if (roleMenus != null && roleMenus.Count > 0)
                {
                    foreach (var rm in roleMenus)
                    {
                        var newRm = new RoleMenu
                        {
                            RoleId = role.Id,
                            MenuId = rm.MenuId,
                            Olusturan = rm.Olusturan ?? Environment.UserName,
                            OlusturmaTarihi = rm.OlusturmaTarihi ?? DateTime.Now
                        };
                        await db.Set<RoleMenu>().AddAsync(newRm, cancellationToken);
                    }
                }

                await scope.CommitAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("RoleService.UpdateWithRelationsAsync error", ex);
                throw;
            }
        }
    }
}
