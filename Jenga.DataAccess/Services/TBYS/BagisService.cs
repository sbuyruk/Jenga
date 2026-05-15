using Jenga.DataAccess.Data;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public class BagisService : IBagisService
    {
        private const string Source = nameof(BagisService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public BagisService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public async Task<Result<List<Bagis>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.Bagis_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result<List<Bagis>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetAllAsync hata.", ex);
                return Result<List<Bagis>>.Failure(Error.Unexpected("Bağış listesi alınamadı.", ex));
            }
        }

        public async Task<Result<List<Bagis>>> GetAllEnvanterdeAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.Bagis_Table.AsNoTracking().Where(b => b.Envanterde).ToListAsync(cancellationToken);
                return Result<List<Bagis>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetAllEnvanterdeAsync hata.", ex);
                return Result<List<Bagis>>.Failure(Error.Unexpected("Envanterdeki bağış listesi alınamadı.", ex));
            }
        }

        public async Task<Result<Bagis>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.Bagis_Table.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
                return entity is null
                    ? Result<Bagis>.Failure(Error.NotFound("Bağış bulunamadı."))
                    : Result<Bagis>.Success(entity);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetByIdAsync hata.", ex);
                return Result<Bagis>.Failure(Error.Unexpected("Bağış alınamadı.", ex));
            }
        }

        public Task<Result<Bagis>> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default)
            => GetByIdAsync(id, cancellationToken);

        public async Task<Result<List<Bagis>>> GetByBagisciIdAsync(int bagisciId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.Bagis_Table.AsNoTracking().Where(b => b.BagisciId == bagisciId).ToListAsync(cancellationToken);
                return Result<List<Bagis>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetByBagisciIdAsync hata.", ex);
                return Result<List<Bagis>>.Failure(Error.Unexpected("Bağışçıya ait bağışlar alınamadı.", ex));
            }
        }

        public async Task<Result<List<Bagis>>> GetByTasinmazIdAsync(int tasinmazId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.Bagis_Table.AsNoTracking().Where(b => b.TasinmazId == tasinmazId).ToListAsync(cancellationToken);
                return Result<List<Bagis>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetByTasinmazIdAsync hata.", ex);
                return Result<List<Bagis>>.Failure(Error.Unexpected("Taşınmaza ait bağışlar alınamadı.", ex));
            }
        }

        public async Task<Result> AddAsync(Bagis bagis, CancellationToken cancellationToken = default)
        {
            if (bagis is null)
                return Result.Failure(Error.Validation("Bağış kaydı boş olamaz."));

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.Bagis_Table.AddAsync(bagis, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.AddAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Bağış eklenemedi.", ex));
            }
        }

        public async Task<Result> UpdateAsync(Bagis bagis, CancellationToken cancellationToken = default)
        {
            if (bagis is null)
                return Result.Failure(Error.Validation("Bağış kaydı boş olamaz."));

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.Bagis_Table.Update(bagis);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.UpdateAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Bağış güncellenemedi.", ex));
            }
        }

        public async Task<Result> DeleteAsync(int bagisId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.Bagis_Table.FirstOrDefaultAsync(b => b.Id == bagisId, cancellationToken);
                if (entity is null)
                    return Result.Failure(Error.NotFound("Silinecek bağış bulunamadı."));

                db.Bagis_Table.Remove(entity);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.DeleteAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Bağış silinemedi.", ex));
            }
        }

        public async Task<Result<bool>> AnyAsync(Expression<Func<Bagis, bool>> predicate, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var any = await db.Bagis_Table.AnyAsync(predicate, cancellationToken);
                return Result<bool>.Success(any);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.AnyAsync hata.", ex);
                return Result<bool>.Failure(Error.Unexpected("Sorgu çalıştırılamadı.", ex));
            }
        }

        public async Task<Result<(bool CanDelete, string? Reason)>> CanDeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var exists = await db.Bagis_Table.AsNoTracking().AnyAsync(b => b.Id == id, cancellationToken);
                if (!exists)
                    return Result<(bool CanDelete, string? Reason)>.Success((false, "Kayıt bulunamadı."));
                return Result<(bool CanDelete, string? Reason)>.Success((true, null));
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.CanDeleteAsync hata.", ex);
                return Result<(bool CanDelete, string? Reason)>.Failure(Error.Unexpected("Silinebilirlik sorgusu başarısız.", ex));
            }
        }
    }
}
