using Jenga.DataAccess.Data;
using Jenga.Models.Inventory;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialBrandService : IMaterialBrandService
    {
        private const string Source = nameof(MaterialBrandService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public MaterialBrandService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService;
        }

        public async Task<Result<List<MaterialBrand>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.MaterialBrand_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.GetAllAsync");
                return Result.Failure<List<MaterialBrand>>(Error.Unexpected("Marka listesi alınamadı.", ex, "MaterialBrand.GetAll.Failed"));
            }
        }

        public async Task<Result<MaterialBrand>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.MaterialBrand_Table.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
                if (entity is null)
                    return Result.Failure<MaterialBrand>(Error.NotFound($"Marka bulunamadı (Id={id}).", "MaterialBrand.NotFound"));
                return Result.Success(entity);
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.GetByIdAsync");
                return Result.Failure<MaterialBrand>(Error.Unexpected("Marka getirilemedi.", ex, "MaterialBrand.GetById.Failed"));
            }
        }

        public async Task<Result> AddAsync(MaterialBrand brand, CancellationToken cancellationToken = default)
        {
            if (brand == null)
                return Result.Failure(Error.Validation("Marka boş olamaz.", "MaterialBrand.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.MaterialBrand_Table.AddAsync(brand, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.AddAsync");
                return Result.Failure(Error.Unexpected("Marka eklenemedi.", ex, "MaterialBrand.Add.Failed"));
            }
        }

        public async Task<Result> UpdateAsync(MaterialBrand brand, CancellationToken cancellationToken = default)
        {
            if (brand == null)
                return Result.Failure(Error.Validation("Marka boş olamaz.", "MaterialBrand.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.MaterialBrand_Table.Update(brand);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.UpdateAsync");
                return Result.Failure(Error.Unexpected("Marka güncellenemedi.", ex, "MaterialBrand.Update.Failed"));
            }
        }

        public async Task<Result> DeleteAsync(MaterialBrand brand, CancellationToken cancellationToken = default)
        {
            if (brand == null)
                return Result.Failure(Error.Validation("Marka boş olamaz.", "MaterialBrand.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.MaterialBrand_Table.FirstOrDefaultAsync(b => b.Id == brand.Id, cancellationToken);
                if (entity == null)
                    return Result.Failure(Error.NotFound($"Marka bulunamadı (Id={brand.Id}).", "MaterialBrand.NotFound"));
                db.MaterialBrand_Table.Remove(entity);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.DeleteAsync");
                return Result.Failure(Error.Unexpected("Marka silinemedi.", ex, "MaterialBrand.Delete.Failed"));
            }
        }

        public async Task<Result<bool>> AnyAsync(Expression<Func<MaterialBrand, bool>> predicate, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var any = await db.MaterialBrand_Table.AsNoTracking().AnyAsync(predicate, cancellationToken);
                return Result.Success(any);
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.AnyAsync");
                return Result.Failure<bool>(Error.Unexpected("Marka sorgusu yapılamadı.", ex, "MaterialBrand.Any.Failed"));
            }
        }
    }
}