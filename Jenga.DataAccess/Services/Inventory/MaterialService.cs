using Jenga.DataAccess.Data;
using Jenga.Models.Inventory;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialService : IMaterialService
    {
        private const string Source = nameof(MaterialService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public MaterialService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public async Task<Result<List<Material>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.Material_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetAllAsync");
                return Result.Failure<List<Material>>(Error.Unexpected("Malzeme listesi alinamadi.", ex, "Material.GetAll.Failed"));
            }
        }

        public async Task<Result<Material>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.Material_Table.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
                if (entity is null)
                    return Result.Failure<Material>(Error.NotFound($"Malzeme bulunamadi (Id={id}).", "Material.NotFound"));
                return Result.Success(entity);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetByIdAsync");
                return Result.Failure<Material>(Error.Unexpected("Malzeme getirilemedi.", ex, "Material.GetById.Failed"));
            }
        }

        public async Task<Result<Material>> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.Material_Table.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
                if (entity is null)
                    return Result.Failure<Material>(Error.NotFound($"Malzeme bulunamadi (Id={id}).", "Material.NotFound"));
                return Result.Success(entity);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetByIdWithRelationsAsync");
                return Result.Failure<Material>(Error.Unexpected("Malzeme getirilemedi.", ex, "Material.GetByIdWithRelations.Failed"));
            }
        }

        public async Task<Result> AddAsync(Material material, CancellationToken cancellationToken = default)
        {
            if (material == null)
                return Result.Failure(Error.Validation("Malzeme bos olamaz.", "Material.Null"));

            var name = (material.MaterialName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure(Error.Validation("Malzeme adi bos olmamali.", "Material.Name.Empty"));

            var existsResult = await ExistsByNameAsync(name, null, cancellationToken);
            if (existsResult.IsFailure) return Result.Failure(existsResult.Error);
            if (existsResult.Value)
                return Result.Failure(Error.Conflict($"Ayni isimde zaten bir malzeme tanimli: '{name}'.", "Material.Name.Duplicate"));

            try
            {
                material.MaterialName = name;
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.Material_Table.AddAsync(material, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                _logService.LogWarning($"{Source}.AddAsync race-condition: '{name}' için unique index ihlali.");
                return Result.Failure(Error.Conflict($"Ayni isimde zaten bir malzeme tanimli: '{name}'.", "Material.Name.Duplicate"));
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.AddAsync");
                return Result.Failure(Error.Unexpected("Malzeme eklenemedi.", ex, "Material.Add.Failed"));
            }
        }

        public async Task<Result> UpdateAsync(Material material, CancellationToken cancellationToken = default)
        {
            if (material == null)
                return Result.Failure(Error.Validation("Malzeme bos olamaz.", "Material.Null"));

            var name = (material.MaterialName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure(Error.Validation("Malzeme adi bos olmamali.", "Material.Name.Empty"));

            var existsResult = await ExistsByNameAsync(name, material.Id, cancellationToken);
            if (existsResult.IsFailure) return Result.Failure(existsResult.Error);
            if (existsResult.Value)
                return Result.Failure(Error.Conflict($"Ayni isimde zaten bir malzeme tanimli: '{name}'.", "Material.Name.Duplicate"));

            try
            {
                material.MaterialName = name;
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.Material_Table.Update(material);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                _logService.LogWarning($"{Source}.UpdateAsync race-condition: '{name}' (id:{material.Id}) için unique index ihlali.");
                return Result.Failure(Error.Conflict($"Ayni isimde zaten bir malzeme tanimli: '{name}'.", "Material.Name.Duplicate"));
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.UpdateAsync");
                return Result.Failure(Error.Unexpected("Malzeme güncellenemedi.", ex, "Material.Update.Failed"));
            }
        }

        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            for (var inner = ex.InnerException; inner != null; inner = inner.InnerException)
            {
                if (inner is Microsoft.Data.SqlClient.SqlException sql &&
                    (sql.Number == 2601 || sql.Number == 2627))
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<Result> DeleteAsync(int materialId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

                if (await db.MaterialEntry_Table.AsNoTracking().AnyAsync(m => m.MaterialId == materialId, cancellationToken))
                    return Result.Failure(Error.Conflict("Bu malzeme giris kayitlarinda kullaniliyor.", "Material.InUse.Entry"));
                if (await db.MaterialExit_Table.AsNoTracking().AnyAsync(m => m.MaterialId == materialId, cancellationToken))
                    return Result.Failure(Error.Conflict("Bu malzeme çikis kayitlarinda kullaniliyor.", "Material.InUse.Exit"));
                if (await db.MaterialInventory_Table.AsNoTracking().AnyAsync(m => m.MaterialId == materialId, cancellationToken))
                    return Result.Failure(Error.Conflict("Bu malzeme envanter kayitlarinda kullaniliyor.", "Material.InUse.Inventory"));

                var entity = await db.Material_Table.FirstOrDefaultAsync(m => m.Id == materialId, cancellationToken);
                if (entity == null)
                    return Result.Failure(Error.NotFound($"Malzeme bulunamadi (Id={materialId}).", "Material.NotFound"));

                db.Material_Table.Remove(entity);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.DeleteAsync");
                return Result.Failure(Error.Unexpected("Malzeme silinemedi.", ex, "Material.Delete.Failed"));
            }
        }

        public async Task<Result<bool>> AnyAsync(Expression<Func<Material, bool>> predicate, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var any = await db.Material_Table.AsNoTracking().AnyAsync(predicate, cancellationToken);
                return Result.Success(any);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.AnyAsync");
                return Result.Failure<bool>(Error.Unexpected("Malzeme sorgusu yapilamadi.", ex, "Material.Any.Failed"));
            }
        }

        public async Task<Result<(bool CanDelete, string? Reason)>> CanDeleteAsync(int id)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync();

                if (await db.MaterialEntry_Table.AsNoTracking().AnyAsync(m => m.MaterialId == id))
                    return Result.Success<(bool CanDelete, string? Reason)>((false, "Bu malzeme envantere giris (MaterialEntry) kayitlarinda bulunmaktadir, önce onu silmelisiniz."));

                if (await db.MaterialExit_Table.AsNoTracking().AnyAsync(m => m.MaterialId == id))
                    return Result.Success<(bool CanDelete, string? Reason)>((false, "Bu malzeme envanterden çikis (MaterialExit) kayitlarinda bulunmaktadir, önce onu silmelisiniz."));

                if (await db.MaterialInventory_Table.AsNoTracking().AnyAsync(m => m.MaterialId == id))
                    return Result.Success<(bool CanDelete, string? Reason)>((false, "Bu malzeme envanter (MaterialInventory) kayitlarinda bulunmaktadir, önce onu silmelisiniz."));

                return Result.Success<(bool CanDelete, string? Reason)>((true, null));
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.CanDeleteAsync");
                return Result.Failure<(bool CanDelete, string? Reason)>(Error.Unexpected("Malzeme silinebilirlik kontrolü yapilamadi.", ex, "Material.CanDelete.Failed"));
            }
        }

        public async Task<Result<bool>> ExistsByNameAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name)) return Result.Success(false);
            var trimmed = name.Trim();

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var exists = await db.Material_Table.AsNoTracking().AnyAsync(m =>
                    m.MaterialName != null &&
                    EF.Functions.Like(m.MaterialName, trimmed) &&
                    (!excludeId.HasValue || m.Id != excludeId.Value),
                    cancellationToken);
                return Result.Success(exists);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.ExistsByNameAsync");
                return Result.Failure<bool>(Error.Unexpected("Malzeme adi kontrolü yapilamadi.", ex, "Material.ExistsByName.Failed"));
            }
        }
    }
}