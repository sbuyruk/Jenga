using Jenga.DataAccess.Data;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public class VasiyetciService : IVasiyetciService
    {
        private const string Source = nameof(VasiyetciService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public VasiyetciService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService;
        }

        public async Task<Result<List<Vasiyetci>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.Vasiyetci_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result<List<Vasiyetci>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.GetAllAsync hata.", ex);
                return Result<List<Vasiyetci>>.Failure(Error.Unexpected("Vasiyetçi listesi alınamadı.", ex));
            }
        }

        public async Task<Result<Vasiyetci>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.Vasiyetci_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                return entity is null
                    ? Result<Vasiyetci>.Failure(Error.NotFound("Vasiyetçi bulunamadı."))
                    : Result<Vasiyetci>.Success(entity);
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.GetByIdAsync hata.", ex);
                return Result<Vasiyetci>.Failure(Error.Unexpected("Vasiyetçi alınamadı.", ex));
            }
        }

        public async Task<Result<List<Vasiyetci>>> GetByTCKimlikAsync(long tcKimlik, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.Vasiyetci_Table.AsNoTracking().Where(x => x.TCKimlikNo == tcKimlik).ToListAsync(cancellationToken);
                return Result<List<Vasiyetci>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.GetByTCKimlikAsync hata.", ex);
                return Result<List<Vasiyetci>>.Failure(Error.Unexpected("TC kimliğe göre vasiyetçi alınamadı.", ex));
            }
        }

        public async Task<Result> AddAsync(Vasiyetci entity, CancellationToken cancellationToken = default)
        {
            if (entity is null)
                return Result.Failure(Error.Validation("Vasiyetçi kaydı boş olamaz."));

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.Vasiyetci_Table.AddAsync(entity, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.AddAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Vasiyetçi eklenemedi.", ex));
            }
        }

        public async Task<Result> UpdateAsync(Vasiyetci entity, CancellationToken cancellationToken = default)
        {
            if (entity is null)
                return Result.Failure(Error.Validation("Vasiyetçi kaydı boş olamaz."));

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.Vasiyetci_Table.Update(entity);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.UpdateAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Vasiyetçi güncellenemedi.", ex));
            }
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.Vasiyetci_Table.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (entity is null)
                    return Result.Failure(Error.NotFound("Silinecek vasiyetçi bulunamadı."));

                db.Vasiyetci_Table.Remove(entity);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.DeleteAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Vasiyetçi silinemedi.", ex));
            }
        }

        public async Task<Result<bool>> AnyAsync(Expression<Func<Vasiyetci, bool>> predicate, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var any = await db.Vasiyetci_Table.AnyAsync(predicate, cancellationToken);
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
