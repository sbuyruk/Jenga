using Jenga.DataAccess.Data;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public class TasinmazService : ITasinmazService
    {
        private const string Source = nameof(TasinmazService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public TasinmazService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public async Task<Result<List<Tasinmaz>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.Tasinmaz_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetAllAsync");
                return Result.Failure<List<Tasinmaz>>(Error.Unexpected("Taşınmazlar getirilemedi.", ex, "Tasinmaz.GetAll.Failed"));
            }
        }

        public async Task<Result<List<Tasinmaz>>> GetByEnvanterDurumuAsync(int envanterdeMi, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.Tasinmaz_Table.AsNoTracking()
                    .Where(x => x.EnvanterdeMi == envanterdeMi)
                    .ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetByEnvanterDurumuAsync");
                return Result.Failure<List<Tasinmaz>>(Error.Unexpected("Envanter durumuna göre taşınmazlar getirilemedi.", ex, "Tasinmaz.GetByEnvanterDurumu.Failed"));
            }
        }

        public Task<Result<List<Tasinmaz>>> GetEnvanterdekilerAsync(CancellationToken cancellationToken = default)
            => GetByEnvanterDurumuAsync(1, cancellationToken);

        public async Task<Result<Tasinmaz>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.Tasinmaz_Table.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
                if (entity is null)
                    return Result.Failure<Tasinmaz>(Error.NotFound($"Taşınmaz bulunamadı (Id={id}).", "Tasinmaz.NotFound"));
                return Result.Success(entity);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetByIdAsync");
                return Result.Failure<Tasinmaz>(Error.Unexpected("Taşınmaz getirilemedi.", ex, "Tasinmaz.GetById.Failed"));
            }
        }

        public async Task<Result> AddAsync(Tasinmaz tasinmaz, CancellationToken cancellationToken = default)
        {
            if (tasinmaz == null)
                return Result.Failure(Error.Validation("Taşınmaz boş olamaz.", "Tasinmaz.Null"));

            var sicil = (tasinmaz.EmlakSicilNo ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(sicil))
            {
                var existsResult = await ExistsByEmlakSicilNoAsync(sicil, null, cancellationToken);
                if (existsResult.IsFailure) return Result.Failure(existsResult.Error);
                if (existsResult.Value)
                {
                    _logService.LogWarning($"{Source}.AddAsync Aynı EmlakSicilNo zaten kayıtlı: '{sicil}'.");
                    return Result.Failure(Error.Conflict($"Aynı EmlakSicilNo zaten kayıtlı: '{sicil}'.", "Tasinmaz.EmlakSicilNo.Duplicate"));
                }
            }

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.Tasinmaz_Table.AddAsync(tasinmaz, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.AddAsync");
                return Result.Failure(Error.Unexpected("Taşınmaz eklenemedi.", ex, "Tasinmaz.Add.Failed"));
            }
        }

        public async Task<Result> UpdateAsync(Tasinmaz tasinmaz, CancellationToken cancellationToken = default)
        {
            if (tasinmaz == null)
                return Result.Failure(Error.Validation("Taşınmaz boş olamaz.", "Tasinmaz.Null"));

            var sicil = (tasinmaz.EmlakSicilNo ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(sicil))
            {
                var existsResult = await ExistsByEmlakSicilNoAsync(sicil, tasinmaz.Id, cancellationToken);
                if (existsResult.IsFailure) return Result.Failure(existsResult.Error);
                if (existsResult.Value)
                {
                    _logService.LogWarning($"{Source}.UpdateAsync Aynı EmlakSicilNo zaten kayıtlı: '{sicil}' (id:{tasinmaz.Id}).");
                    return Result.Failure(Error.Conflict($"Aynı EmlakSicilNo zaten kayıtlı: '{sicil}'.", "Tasinmaz.EmlakSicilNo.Duplicate"));
                }
            }

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.Tasinmaz_Table.Update(tasinmaz);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.UpdateAsync");
                return Result.Failure(Error.Unexpected("Taşınmaz güncellenemedi.", ex, "Tasinmaz.Update.Failed"));
            }
        }

        public async Task<Result> DeleteAsync(int tasinmazId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.Tasinmaz_Table.FirstOrDefaultAsync(x => x.Id == tasinmazId, cancellationToken);
                if (entity == null)
                    return Result.Failure(Error.NotFound($"Taşınmaz bulunamadı (Id={tasinmazId}).", "Tasinmaz.NotFound"));

                db.Tasinmaz_Table.Remove(entity);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.DeleteAsync");
                return Result.Failure(Error.Unexpected("Taşınmaz silinemedi.", ex, "Tasinmaz.Delete.Failed"));
            }
        }

        public async Task<Result<bool>> AnyAsync(Expression<Func<Tasinmaz, bool>> predicate, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var any = await db.Tasinmaz_Table.AnyAsync(predicate, cancellationToken);
                return Result.Success(any);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.AnyAsync");
                return Result.Failure<bool>(Error.Unexpected("Taşınmaz sorgusu yapılamadı.", ex, "Tasinmaz.Any.Failed"));
            }
        }

        public async Task<Result<string>> GetEmlakSicilNoAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var sicilNo = await db.Tasinmaz_Table.AsNoTracking()
                    .Where(x => x.Id == id)
                    .Select(x => x.EmlakSicilNo)
                    .FirstOrDefaultAsync(cancellationToken);
                return Result.Success(sicilNo ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetEmlakSicilNoAsync");
                return Result.Failure<string>(Error.Unexpected("EmlakSicilNo getirilemedi.", ex, "Tasinmaz.GetEmlakSicilNo.Failed"));
            }
        }

        public Task<Result<(bool CanDelete, string? Reason)>> CanDeleteAsync(int id, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success<(bool CanDelete, string? Reason)>((true, null)));

        public async Task<Result<bool>> ExistsByEmlakSicilNoAsync(string emlakSicilNo, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(emlakSicilNo))
                return Result.Success(false);

            try
            {
                var normalized = emlakSicilNo.Trim();

                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var exists = await db.Tasinmaz_Table.AsNoTracking().AnyAsync(m =>
                    m.EmlakSicilNo != null &&
                    m.EmlakSicilNo.Trim() == normalized &&
                    (!excludeId.HasValue || m.Id != excludeId.Value), cancellationToken);
                return Result.Success(exists);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.ExistsByEmlakSicilNoAsync");
                return Result.Failure<bool>(Error.Unexpected("EmlakSicilNo kontrolü yapılamadı.", ex, "Tasinmaz.ExistsByEmlakSicilNo.Failed"));
            }
        }
    }
}
