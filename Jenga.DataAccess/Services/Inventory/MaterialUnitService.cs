using Jenga.DataAccess.Data;
using Jenga.Models.Inventory;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialUnitService : IMaterialUnitService
    {
        private const string Source = nameof(MaterialUnitService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;
        private List<MaterialUnit>? _unitsCache;

        public MaterialUnitService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService;
        }

        public async Task<Result<List<MaterialUnit>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.MaterialUnit_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.GetAllAsync");
                return Result.Failure<List<MaterialUnit>>(Error.Unexpected("Birim listesi alınamadı.", ex, "MaterialUnit.GetAll.Failed"));
            }
        }

        public async Task<Result<MaterialUnit>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.MaterialUnit_Table.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
                if (entity is null)
                    return Result.Failure<MaterialUnit>(Error.NotFound($"Birim bulunamadı (Id={id}).", "MaterialUnit.NotFound"));
                return Result.Success(entity);
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.GetByIdAsync");
                return Result.Failure<MaterialUnit>(Error.Unexpected("Birim getirilemedi.", ex, "MaterialUnit.GetById.Failed"));
            }
        }

        public async Task<Result> AddAsync(MaterialUnit unit, CancellationToken cancellationToken = default)
        {
            if (unit == null)
                return Result.Failure(Error.Validation("Birim boş olamaz.", "MaterialUnit.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.MaterialUnit_Table.AddAsync(unit, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                _unitsCache = null;
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.AddAsync");
                return Result.Failure(Error.Unexpected("Birim eklenemedi.", ex, "MaterialUnit.Add.Failed"));
            }
        }

        public async Task<Result> UpdateAsync(MaterialUnit unit, CancellationToken cancellationToken = default)
        {
            if (unit == null)
                return Result.Failure(Error.Validation("Birim boş olamaz.", "MaterialUnit.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.MaterialUnit_Table.Update(unit);
                await db.SaveChangesAsync(cancellationToken);
                _unitsCache = null;
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.UpdateAsync");
                return Result.Failure(Error.Unexpected("Birim güncellenemedi.", ex, "MaterialUnit.Update.Failed"));
            }
        }

        public async Task<Result> DeleteAsync(MaterialUnit unit, CancellationToken cancellationToken = default)
        {
            if (unit == null)
                return Result.Failure(Error.Validation("Birim boş olamaz.", "MaterialUnit.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.MaterialUnit_Table.FirstOrDefaultAsync(u => u.Id == unit.Id, cancellationToken);
                if (entity == null)
                    return Result.Failure(Error.NotFound($"Birim bulunamadı (Id={unit.Id}).", "MaterialUnit.NotFound"));
                db.MaterialUnit_Table.Remove(entity);
                await db.SaveChangesAsync(cancellationToken);
                _unitsCache = null;
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.DeleteAsync");
                return Result.Failure(Error.Unexpected("Birim silinemedi.", ex, "MaterialUnit.Delete.Failed"));
            }
        }

        // Yardımcı Metot
        public async Task<string> GetUnitSymbolAsync(int unitId, CancellationToken cancellationToken = default)
        {
            if (_unitsCache == null)
            {
                var allResult = await GetAllAsync(cancellationToken);
                if (allResult.IsFailure) return "";
                _unitsCache = allResult.Value;
            }
            var unit = _unitsCache.FirstOrDefault(x => x.Id == unitId);
            return unit?.Symbol ?? "";
        }
    }
}