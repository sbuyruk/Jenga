using Jenga.DataAccess.Data;
using Jenga.Models.Common;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Common
{
    public class BolgeService : IBolgeService
    {
        private const string Source = nameof(BolgeService);
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;
        private List<Bolge>? _cache;

        public BolgeService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public async Task<Result<List<Bolge>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_cache == null)
                {
                    await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                    _cache = await db.Bolge_Table.AsNoTracking().ToListAsync(cancellationToken);
                }
                return Result.Success(_cache);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetAllAsync");
                return Result.Failure<List<Bolge>>(Error.Unexpected("Bölgeler getirilemedi.", ex, "Bolge.GetAll.Failed"));
            }
        }

        public async Task<Result<Bolge>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.Bolge_Table.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
                if (entity is null)
                    return Result.Failure<Bolge>(Error.NotFound($"Bölge bulunamadi (Id={id}).", "Bolge.NotFound"));
                return Result.Success(entity);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetByIdAsync");
                return Result.Failure<Bolge>(Error.Unexpected("Bölge getirilemedi.", ex, "Bolge.GetById.Failed"));
            }
        }

        public async Task<Result<Bolge>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure<Bolge>(Error.Validation("Isim bos olamaz.", "Bolge.Name.Empty"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var trimmed = name.Trim();
                var entity = await db.Bolge_Table.AsNoTracking().FirstOrDefaultAsync(b => b.Adi == trimmed, cancellationToken);
                if (entity is null)
                    return Result.Failure<Bolge>(Error.NotFound($"Bölge bulunamadi (Adi={trimmed}).", "Bolge.NotFound"));
                return Result.Success(entity);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetByNameAsync");
                return Result.Failure<Bolge>(Error.Unexpected("Bölge getirilemedi.", ex, "Bolge.GetByName.Failed"));
            }
        }

        public async Task<Result> AddAsync(Bolge bolge, CancellationToken cancellationToken = default)
        {
            if (bolge == null)
                return Result.Failure(Error.Validation("Bölge bos olamaz.", "Bolge.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.Bolge_Table.AddAsync(bolge, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                _cache = null;
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.AddAsync");
                return Result.Failure(Error.Unexpected("Bölge eklenemedi.", ex, "Bolge.Add.Failed"));
            }
        }

        public async Task<Result> UpdateAsync(Bolge bolge, CancellationToken cancellationToken = default)
        {
            if (bolge == null)
                return Result.Failure(Error.Validation("Bölge bos olamaz.", "Bolge.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.Bolge_Table.Update(bolge);
                await db.SaveChangesAsync(cancellationToken);
                _cache = null;
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.UpdateAsync");
                return Result.Failure(Error.Unexpected("Bölge güncellenemedi.", ex, "Bolge.Update.Failed"));
            }
        }

        public async Task<Result> DeleteAsync(int bolgeId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.Bolge_Table.FirstOrDefaultAsync(b => b.Id == bolgeId, cancellationToken);
                if (entity == null)
                    return Result.Failure(Error.NotFound($"Bölge bulunamadi (Id={bolgeId}).", "Bolge.NotFound"));

                db.Bolge_Table.Remove(entity);
                await db.SaveChangesAsync(cancellationToken);
                _cache = null;
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.DeleteAsync");
                return Result.Failure(Error.Unexpected("Bölge silinemedi.", ex, "Bolge.Delete.Failed"));
            }
        }

        public async Task<Result<List<Bolge>>> GetAktifBolgelerAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.Bolge_Table.AsNoTracking().Where(b => b.Aktif && b.TemsilcilikMi).ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetAktifBolgelerAsync");
                return Result.Failure<List<Bolge>>(Error.Unexpected("Aktif bölgeler getirilemedi.", ex, "Bolge.GetAktif.Failed"));
            }
        }

        public async Task<Result<bool>> AnyAsync(Expression<Func<Bolge, bool>> predicate, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var any = await db.Bolge_Table.AsNoTracking().AnyAsync(predicate, cancellationToken);
                return Result.Success(any);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.AnyAsync");
                return Result.Failure<bool>(Error.Unexpected("Bölge sorgusu basarisiz.", ex, "Bolge.Any.Failed"));
            }
        }
    }
}
