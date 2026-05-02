using Jenga.DataAccess.Data;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.TBYS
{
    public class TasinmazTaahhutService : ITasinmazTaahhutService
    {
        private const string Source = nameof(TasinmazTaahhutService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public TasinmazTaahhutService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public async Task<Result<List<TasinmazTaahhut>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.TasinmazTaahhut_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result<List<TasinmazTaahhut>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetAllAsync hata.", ex);
                return Result<List<TasinmazTaahhut>>.Failure(Error.Unexpected("Taahhüt listesi alınamadı.", ex));
            }
        }

        public async Task<Result<TasinmazTaahhut>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.TasinmazTaahhut_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                return entity is null
                    ? Result<TasinmazTaahhut>.Failure(Error.NotFound("Taahhüt bulunamadı."))
                    : Result<TasinmazTaahhut>.Success(entity);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetByIdAsync hata.", ex);
                return Result<TasinmazTaahhut>.Failure(Error.Unexpected("Taahhüt alınamadı.", ex));
            }
        }

        public async Task<Result<List<TasinmazTaahhut>>> GetByTasinmazIdAsync(int tasinmazId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.TasinmazTaahhut_Table.AsNoTracking().Where(x => x.TasinmazId == tasinmazId).ToListAsync(cancellationToken);
                return Result<List<TasinmazTaahhut>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetByTasinmazIdAsync hata.", ex);
                return Result<List<TasinmazTaahhut>>.Failure(Error.Unexpected("Taşınmaza ait taahhütler alınamadı.", ex));
            }
        }

        public async Task<Result<List<TasinmazTaahhut>>> GetByBagisciIdAsync(int bagisciId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.TasinmazTaahhut_Table.AsNoTracking().Where(x => x.BagisciId == bagisciId).ToListAsync(cancellationToken);
                return Result<List<TasinmazTaahhut>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetByBagisciIdAsync hata.", ex);
                return Result<List<TasinmazTaahhut>>.Failure(Error.Unexpected("Bağışçıya ait taahhütler alınamadı.", ex));
            }
        }

        public async Task<Result> AddAsync(TasinmazTaahhut entity, CancellationToken cancellationToken = default)
        {
            if (entity is null)
                return Result.Failure(Error.Validation("Taahhüt kaydı boş olamaz."));

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.TasinmazTaahhut_Table.AddAsync(entity, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.AddAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Taahhüt eklenemedi.", ex));
            }
        }

        public async Task<Result> UpdateAsync(TasinmazTaahhut entity, CancellationToken cancellationToken = default)
        {
            if (entity is null)
                return Result.Failure(Error.Validation("Taahhüt kaydı boş olamaz."));

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.TasinmazTaahhut_Table.Update(entity);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.UpdateAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Taahhüt güncellenemedi.", ex));
            }
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.TasinmazTaahhut_Table.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (entity is null)
                    return Result.Failure(Error.NotFound("Silinecek taahhüt bulunamadı."));

                db.TasinmazTaahhut_Table.Remove(entity);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.DeleteAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Taahhüt silinemedi.", ex));
            }
        }
    }
}
