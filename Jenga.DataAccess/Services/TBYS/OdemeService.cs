using Jenga.DataAccess.Data;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public class OdemeService : IOdemeService
    {
        private const string Source = nameof(OdemeService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public OdemeService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public async Task<Result<List<Odeme>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.Odeme_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result<List<Odeme>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetAllAsync hata.", ex);
                return Result<List<Odeme>>.Failure(Error.Unexpected("Ödeme listesi alınamadı.", ex));
            }
        }

        public async Task<Result<List<Odeme>>> GetAllAsyncKiralar(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.Odeme_Table
                    .Where(o => o.OdemePlaniId != null)
                    .Join(db.OdemePlani_Table,
                          o => o.OdemePlaniId,
                          p => p.Id,
                          (o, p) => o)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
                return Result<List<Odeme>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetAllAsyncKiralar hata.", ex);
                return Result<List<Odeme>>.Failure(Error.Unexpected("Kira ödemeleri alınamadı.", ex));
            }
        }

        public async Task<Result<Odeme>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.Odeme_Table.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
                return entity is null
                    ? Result<Odeme>.Failure(Error.NotFound("Ödeme bulunamadı."))
                    : Result<Odeme>.Success(entity);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetByIdAsync hata.", ex);
                return Result<Odeme>.Failure(Error.Unexpected("Ödeme alınamadı.", ex));
            }
        }

        public async Task<Result<Odeme>> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.Odeme_Table.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
                return entity is null
                    ? Result<Odeme>.Failure(Error.NotFound("Ödeme bulunamadı."))
                    : Result<Odeme>.Success(entity);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetByIdWithRelationsAsync hata.", ex);
                return Result<Odeme>.Failure(Error.Unexpected("Ödeme alınamadı.", ex));
            }
        }

        public async Task<Result<List<Odeme>>> GetBySozlesmeIdAsync(int sozlesmeId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.Odeme_Table.AsNoTracking().Where(o => o.SozlesmeId == sozlesmeId).ToListAsync(cancellationToken);
                return Result<List<Odeme>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetBySozlesmeIdAsync hata.", ex);
                return Result<List<Odeme>>.Failure(Error.Unexpected("Sözleşmeye ait ödemeler alınamadı.", ex));
            }
        }

        public async Task<Result<List<Odeme>>> GetByKiraciIdAsync(int kiraciId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.Odeme_Table.AsNoTracking().Where(o => o.KiraciId == kiraciId).ToListAsync(cancellationToken);
                return Result<List<Odeme>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetByKiraciIdAsync hata.", ex);
                return Result<List<Odeme>>.Failure(Error.Unexpected("Kiracıya ait ödemeler alınamadı.", ex));
            }
        }

        public async Task<Result<List<Odeme>>> GetByOdemePlaniIdAsync(int odemePlaniId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.Odeme_Table.AsNoTracking().Where(o => o.OdemePlaniId == odemePlaniId).ToListAsync(cancellationToken);
                return Result<List<Odeme>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetByOdemePlaniIdAsync hata.", ex);
                return Result<List<Odeme>>.Failure(Error.Unexpected("Ödeme planına ait ödemeler alınamadı.", ex));
            }
        }

        public async Task<Result> AddAsync(Odeme odeme, CancellationToken cancellationToken = default)
        {
            if (odeme is null)
                return Result.Failure(Error.Validation("Ödeme kaydı boş olamaz."));

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.Odeme_Table.AddAsync(odeme, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.AddAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Ödeme eklenemedi.", ex));
            }
        }

        public async Task<Result> UpdateAsync(Odeme odeme, CancellationToken cancellationToken = default)
        {
            if (odeme is null)
                return Result.Failure(Error.Validation("Ödeme kaydı boş olamaz."));

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.Odeme_Table.Update(odeme);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.UpdateAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Ödeme güncellenemedi.", ex));
            }
        }

        public async Task<Result> DeleteAsync(int odemeId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.Odeme_Table.FirstOrDefaultAsync(o => o.Id == odemeId, cancellationToken);
                if (entity is null)
                    return Result.Failure(Error.NotFound("Silinecek ödeme bulunamadı."));

                db.Odeme_Table.Remove(entity);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.DeleteAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Ödeme silinemedi.", ex));
            }
        }

        public async Task<Result<bool>> AnyAsync(Expression<Func<Odeme, bool>> predicate, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var any = await db.Odeme_Table.AnyAsync(predicate, cancellationToken);
                return Result<bool>.Success(any);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.AnyAsync hata.", ex);
                return Result<bool>.Failure(Error.Unexpected("Ödeme sorgulanamadı.", ex));
            }
        }

        public async Task<Result<(bool CanDelete, string? Reason)>> CanDeleteAsync(int id)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync();
                var exists = await db.Odeme_Table.AsNoTracking().AnyAsync(o => o.Id == id);
                return exists
                    ? Result<(bool CanDelete, string? Reason)>.Success((true, null))
                    : Result<(bool CanDelete, string? Reason)>.Success((false, "Kayıt bulunamadı."));
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.CanDeleteAsync hata.", ex);
                return Result<(bool CanDelete, string? Reason)>.Failure(Error.Unexpected("Silme kontrolü yapılamadı.", ex));
            }
        }
    }
}
