using Jenga.DataAccess.Data;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public class SozlesmeTasinmazService : ISozlesmeTasinmazService
    {
        private const string Source = nameof(SozlesmeTasinmazService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public SozlesmeTasinmazService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService;
        }

        public async Task<Result<List<SozlesmeTasinmaz>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.SozlesmeTasinmaz_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result<List<SozlesmeTasinmaz>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.GetAllAsync hata.", ex);
                return Result<List<SozlesmeTasinmaz>>.Failure(Error.Unexpected("Sözleşme-taşınmaz listesi alınamadı.", ex));
            }
        }

        public async Task<Result<SozlesmeTasinmaz>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.SozlesmeTasinmaz_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                return entity is null
                    ? Result<SozlesmeTasinmaz>.Failure(Error.NotFound("Sözleşme-taşınmaz kaydı bulunamadı."))
                    : Result<SozlesmeTasinmaz>.Success(entity);
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.GetByIdAsync hata.", ex);
                return Result<SozlesmeTasinmaz>.Failure(Error.Unexpected("Sözleşme-taşınmaz kaydı alınamadı.", ex));
            }
        }

        public async Task<Result> AddAsync(SozlesmeTasinmaz entity, CancellationToken cancellationToken = default)
        {
            if (entity is null)
                return Result.Failure(Error.Validation("Sözleşme-taşınmaz kaydı boş olamaz."));

            if (!entity.SozlesmeId.HasValue && !entity.TasinmazId.HasValue)
                return Result.Failure(Error.Validation("SozlesmeId veya TasinmazId gerekli."));

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.SozlesmeTasinmaz_Table.AddAsync(entity, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.AddAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Sözleşme-taşınmaz kaydı eklenemedi.", ex));
            }
        }

        public async Task<Result> UpdateAsync(SozlesmeTasinmaz entity, CancellationToken cancellationToken = default)
        {
            if (entity is null)
                return Result.Failure(Error.Validation("Sözleşme-taşınmaz kaydı boş olamaz."));

            if (!entity.SozlesmeId.HasValue && !entity.TasinmazId.HasValue)
                return Result.Failure(Error.Validation("SozlesmeId veya TasinmazId gerekli."));

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.SozlesmeTasinmaz_Table.Update(entity);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.UpdateAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Sözleşme-taşınmaz kaydı güncellenemedi.", ex));
            }
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.SozlesmeTasinmaz_Table.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (entity is null)
                    return Result.Failure(Error.NotFound("Silinecek sözleşme-taşınmaz kaydı bulunamadı."));

                db.SozlesmeTasinmaz_Table.Remove(entity);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.DeleteAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Sözleşme-taşınmaz kaydı silinemedi.", ex));
            }
        }

        public async Task<Result<bool>> AnyAsync(Expression<Func<SozlesmeTasinmaz, bool>> predicate, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var any = await db.SozlesmeTasinmaz_Table.AnyAsync(predicate, cancellationToken);
                return Result<bool>.Success(any);
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.AnyAsync hata.", ex);
                return Result<bool>.Failure(Error.Unexpected("Sözleşme-taşınmaz sorgulanamadı.", ex));
            }
        }
    }
}
