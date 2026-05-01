using Jenga.DataAccess.Data;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public class BagisciYakinlariService : IBagisciYakinlariService
    {
        private const string Source = nameof(BagisciYakinlariService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public BagisciYakinlariService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService;
        }

        public async Task<Result<List<BagisciYakinlari>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.BagisciYakinlari_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result<List<BagisciYakinlari>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.GetAllAsync hata.", ex);
                return Result<List<BagisciYakinlari>>.Failure(Error.Unexpected("Yakın listesi alınamadı.", ex));
            }
        }

        public async Task<Result<BagisciYakinlari>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.BagisciYakinlari_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                return entity is null
                    ? Result<BagisciYakinlari>.Failure(Error.NotFound("Yakın bulunamadı."))
                    : Result<BagisciYakinlari>.Success(entity);
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.GetByIdAsync hata.", ex);
                return Result<BagisciYakinlari>.Failure(Error.Unexpected("Yakın alınamadı.", ex));
            }
        }

        public async Task<Result<List<BagisciYakinlari>>> GetByBagisciIdAsync(long bagisciId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.BagisciYakinlari_Table.AsNoTracking()
                    .Where(x => x.BagisciId == bagisciId)
                    .OrderBy(x => x.Sira)
                    .ToListAsync(cancellationToken);
                return Result<List<BagisciYakinlari>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.GetByBagisciIdAsync hata.", ex);
                return Result<List<BagisciYakinlari>>.Failure(Error.Unexpected("Bağışçıya ait yakınlar alınamadı.", ex));
            }
        }

        public async Task<Result> AddAsync(BagisciYakinlari entity, CancellationToken cancellationToken = default)
        {
            if (entity is null)
                return Result.Failure(Error.Validation("Yakın kaydı boş olamaz."));

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.BagisciYakinlari_Table.AddAsync(entity, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.AddAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Yakın eklenemedi.", ex));
            }
        }

        public async Task<Result> UpdateAsync(BagisciYakinlari entity, CancellationToken cancellationToken = default)
        {
            if (entity is null)
                return Result.Failure(Error.Validation("Yakın kaydı boş olamaz."));

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.BagisciYakinlari_Table.Update(entity);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.UpdateAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Yakın güncellenemedi.", ex));
            }
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.BagisciYakinlari_Table.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (entity is null)
                    return Result.Failure(Error.NotFound("Silinecek yakın bulunamadı."));

                db.BagisciYakinlari_Table.Remove(entity);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.DeleteAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Yakın silinemedi.", ex));
            }
        }

        public async Task<Result<bool>> AnyAsync(Expression<Func<BagisciYakinlari, bool>> predicate, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var any = await db.BagisciYakinlari_Table.AnyAsync(predicate, cancellationToken);
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
