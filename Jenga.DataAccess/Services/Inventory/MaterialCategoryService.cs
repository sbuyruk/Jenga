using Jenga.DataAccess.Data;
using Jenga.Models.Inventory;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialCategoryService : IMaterialCategoryService
    {
        private const string Source = nameof(MaterialCategoryService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public MaterialCategoryService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public async Task<Result<List<MaterialCategory>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.MaterialCategory_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetAllAsync");
                return Result.Failure<List<MaterialCategory>>(Error.Unexpected("Kategori listesi alinamadi.", ex, "MaterialCategory.GetAll.Failed"));
            }
        }

        public async Task<Result<MaterialCategory>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.MaterialCategory_Table.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
                if (entity is null)
                    return Result.Failure<MaterialCategory>(Error.NotFound($"Kategori bulunamadi (Id={id}).", "MaterialCategory.NotFound"));
                return Result.Success(entity);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetByIdAsync");
                return Result.Failure<MaterialCategory>(Error.Unexpected("Kategori getirilemedi.", ex, "MaterialCategory.GetById.Failed"));
            }
        }

        public async Task<Result> AddAsync(MaterialCategory category, CancellationToken cancellationToken = default)
        {
            if (category == null)
                return Result.Failure(Error.Validation("Kategori bos olamaz.", "MaterialCategory.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.MaterialCategory_Table.AddAsync(category, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.AddAsync");
                return Result.Failure(Error.Unexpected("Kategori eklenemedi.", ex, "MaterialCategory.Add.Failed"));
            }
        }

        public async Task<Result> UpdateAsync(MaterialCategory category, CancellationToken cancellationToken = default)
        {
            if (category == null)
                return Result.Failure(Error.Validation("Kategori bos olamaz.", "MaterialCategory.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.MaterialCategory_Table.Update(category);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.UpdateAsync");
                return Result.Failure(Error.Unexpected("Kategori güncellenemedi.", ex, "MaterialCategory.Update.Failed"));
            }
        }

        public async Task<Result> DeleteAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

                if (await db.MaterialCategory_Table.AsNoTracking().AnyAsync(m => m.ParentCategoryId == categoryId, cancellationToken))
                    return Result.Failure(Error.Conflict("Bu kategori bir alt kategori tarafindan kullaniliyor.", "MaterialCategory.HasChildren"));

                var entity = await db.MaterialCategory_Table.FirstOrDefaultAsync(m => m.Id == categoryId, cancellationToken);
                if (entity == null)
                    return Result.Failure(Error.NotFound($"Kategori bulunamadi (Id={categoryId}).", "MaterialCategory.NotFound"));

                db.MaterialCategory_Table.Remove(entity);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.DeleteAsync");
                return Result.Failure(Error.Unexpected("Kategori silinemedi.", ex, "MaterialCategory.Delete.Failed"));
            }
        }

        public async Task<Result<bool>> AnyAsync(Expression<Func<MaterialCategory, bool>> predicate, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var any = await db.MaterialCategory_Table.AsNoTracking().AnyAsync(predicate, cancellationToken);
                return Result.Success(any);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.AnyAsync");
                return Result.Failure<bool>(Error.Unexpected("Kategori sorgusu yapilamadi.", ex, "MaterialCategory.Any.Failed"));
            }
        }

        public async Task<Result<(bool CanDelete, string? Reason)>> CanDeleteAsync(int id)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync();

                if (await db.MaterialCategory_Table.AsNoTracking().AnyAsync(m => m.ParentCategoryId == id))
                    return Result.Success<(bool CanDelete, string? Reason)>((false, "Bu kategori bir malzemenin üst kategorisi olarak kullaniliyor, önce onu silmelisiniz."));
                if (await db.Material_Table.AsNoTracking().AnyAsync(m => m.CategoryId == id))
                    return Result.Success<(bool CanDelete, string? Reason)>((false, "Bu kategori bir malzemenin kategorisi olarak kullaniliyor, önce onu silmelisiniz."));

                return Result.Success<(bool CanDelete, string? Reason)>((true, null));
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.CanDeleteAsync");
                return Result.Failure<(bool CanDelete, string? Reason)>(Error.Unexpected("Kategori silinebilirlik kontrolü yapilamadi.", ex, "MaterialCategory.CanDelete.Failed"));
            }
        }
    }
}