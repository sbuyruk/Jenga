using Jenga.DataAccess.Data;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public class BagisciTalepleriService : IBagisciTalepleriService
    {
        private const string Source = nameof(BagisciTalepleriService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public BagisciTalepleriService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService;
        }

        public async Task<Result<List<BagisciTalepleri>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.BagisciTalepleri_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result<List<BagisciTalepleri>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.GetAllAsync hata.", ex);
                return Result<List<BagisciTalepleri>>.Failure(Error.Unexpected("Talep listesi alınamadı.", ex));
            }
        }

        public async Task<Result<BagisciTalepleri>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.BagisciTalepleri_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                return entity is null
                    ? Result<BagisciTalepleri>.Failure(Error.NotFound("Talep bulunamadı."))
                    : Result<BagisciTalepleri>.Success(entity);
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.GetByIdAsync hata.", ex);
                return Result<BagisciTalepleri>.Failure(Error.Unexpected("Talep alınamadı.", ex));
            }
        }

        public async Task<Result<List<BagisciTalepleri>>> GetByBagisciIdAsync(long bagisciId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.BagisciTalepleri_Table.AsNoTracking().Where(x => x.BagisciId == bagisciId).ToListAsync(cancellationToken);
                return Result<List<BagisciTalepleri>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.GetByBagisciIdAsync hata.", ex);
                return Result<List<BagisciTalepleri>>.Failure(Error.Unexpected("Bağışçıya ait talepler alınamadı.", ex));
            }
        }

        public async Task<Result> AddAsync(BagisciTalepleri entity, CancellationToken cancellationToken = default)
        {
            if (entity is null)
                return Result.Failure(Error.Validation("Talep kaydı boş olamaz."));

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.BagisciTalepleri_Table.AddAsync(entity, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.AddAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Talep eklenemedi.", ex));
            }
        }

        public async Task<Result> UpdateAsync(BagisciTalepleri entity, CancellationToken cancellationToken = default)
        {
            if (entity is null)
                return Result.Failure(Error.Validation("Talep kaydı boş olamaz."));

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.BagisciTalepleri_Table.Update(entity);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.UpdateAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Talep güncellenemedi.", ex));
            }
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.BagisciTalepleri_Table.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (entity is null)
                    return Result.Failure(Error.NotFound("Silinecek talep bulunamadı."));

                db.BagisciTalepleri_Table.Remove(entity);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.DeleteAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Talep silinemedi.", ex));
            }
        }

        public async Task<Result<bool>> AnyAsync(Expression<Func<BagisciTalepleri, bool>> predicate, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var any = await db.BagisciTalepleri_Table.AnyAsync(predicate, cancellationToken);
                return Result<bool>.Success(any);
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.AnyAsync hata.", ex);
                return Result<bool>.Failure(Error.Unexpected("Sorgu çalıştırılamadı.", ex));
            }
        }
    }
}
