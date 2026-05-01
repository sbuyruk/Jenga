using Jenga.DataAccess.Data;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public class KiraciService : IKiraciService
    {
        private const string Source = nameof(KiraciService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public KiraciService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService;
        }

        public async Task<Result<List<Kiraci>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.Kiraci_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result<List<Kiraci>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.GetAllAsync hata.", ex);
                return Result<List<Kiraci>>.Failure(Error.Unexpected("Kiracı listesi alınamadı.", ex));
            }
        }

        public async Task<Result<Kiraci>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.Kiraci_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                return entity is null
                    ? Result<Kiraci>.Failure(Error.NotFound("Kiracı bulunamadı."))
                    : Result<Kiraci>.Success(entity);
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.GetByIdAsync hata.", ex);
                return Result<Kiraci>.Failure(Error.Unexpected("Kiracı alınamadı.", ex));
            }
        }

        public async Task<Result> AddAsync(Kiraci kiraci, CancellationToken cancellationToken = default)
        {
            if (kiraci is null)
                return Result.Failure(Error.Validation("Kiracı kaydı boş olamaz."));

            var name = (kiraci.Adi ?? string.Empty).Trim();
            var surname = (kiraci.Soyadi ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(surname))
                return Result.Failure(Error.Validation("Adi veya Soyadi boş olamaz."));

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.Kiraci_Table.AddAsync(kiraci, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.AddAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Kiracı eklenemedi.", ex));
            }
        }

        public async Task<Result> UpdateAsync(Kiraci kiraci, CancellationToken cancellationToken = default)
        {
            if (kiraci is null)
                return Result.Failure(Error.Validation("Kiracı kaydı boş olamaz."));

            var name = (kiraci.Adi ?? string.Empty).Trim();
            var surname = (kiraci.Soyadi ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(surname))
                return Result.Failure(Error.Validation("Adi veya Soyadi boş olamaz."));

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.Kiraci_Table.Update(kiraci);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.UpdateAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Kiracı güncellenemedi.", ex));
            }
        }

        public async Task<Result> DeleteAsync(int kiraciId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.Kiraci_Table.FirstOrDefaultAsync(x => x.Id == kiraciId, cancellationToken);
                if (entity is null)
                    return Result.Failure(Error.NotFound("Silinecek kiracı bulunamadı."));

                db.Kiraci_Table.Remove(entity);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.DeleteAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Kiracı silinemedi.", ex));
            }
        }

        public async Task<Result<bool>> AnyAsync(Expression<Func<Kiraci, bool>> predicate, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var any = await db.Kiraci_Table.AnyAsync(predicate, cancellationToken);
                return Result<bool>.Success(any);
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.AnyAsync hata.", ex);
                return Result<bool>.Failure(Error.Unexpected("Kiracı sorgulanamadı.", ex));
            }
        }
    }
}
