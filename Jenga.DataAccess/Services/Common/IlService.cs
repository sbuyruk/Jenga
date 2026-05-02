using Jenga.DataAccess.Data;
using Jenga.Models.Common;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Common
{
    public class IlService : IIlService
    {
        private const string Source = nameof(IlService);
        private static readonly string[] _excludedIlAdlari = { " ", "Bos", "Yok", "---", "Yurtdisi", "Almanya", "Diger" };

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;
        private List<Il>? _cache;

        public IlService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public async Task<Result<List<Il>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_cache == null)
                {
                    await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                    _cache = await db.Il_Table.AsNoTracking().ToListAsync(cancellationToken);
                }
                return Result.Success(_cache);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetAllAsync");
                return Result.Failure<List<Il>>(Error.Unexpected("Iller getirilemedi.", ex, "Il.GetAll.Failed"));
            }
        }

        public async Task<Result<Il>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.Il_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (entity is null)
                    return Result.Failure<Il>(Error.NotFound($"Il bulunamadi (Id={id}).", "Il.NotFound"));
                return Result.Success(entity);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetByIdAsync");
                return Result.Failure<Il>(Error.Unexpected("Il getirilemedi.", ex, "Il.GetById.Failed"));
            }
        }

        public async Task<Result> AddAsync(Il il, CancellationToken cancellationToken = default)
        {
            if (il == null)
                return Result.Failure(Error.Validation("Il bos olamaz.", "Il.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.Il_Table.AddAsync(il, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                _cache = null;
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.AddAsync");
                return Result.Failure(Error.Unexpected("Il eklenemedi.", ex, "Il.Add.Failed"));
            }
        }

        public async Task<Result> UpdateAsync(Il il, CancellationToken cancellationToken = default)
        {
            if (il == null)
                return Result.Failure(Error.Validation("Il bos olamaz.", "Il.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.Il_Table.Update(il);
                await db.SaveChangesAsync(cancellationToken);
                _cache = null;
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.UpdateAsync");
                return Result.Failure(Error.Unexpected("Il güncellenemedi.", ex, "Il.Update.Failed"));
            }
        }

        public async Task<Result> DeleteAsync(int ilId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.Il_Table.FirstOrDefaultAsync(x => x.Id == ilId, cancellationToken);
                if (entity == null)
                    return Result.Failure(Error.NotFound($"Il bulunamadi (Id={ilId}).", "Il.NotFound"));

                db.Il_Table.Remove(entity);
                await db.SaveChangesAsync(cancellationToken);
                _cache = null;
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.DeleteAsync");
                return Result.Failure(Error.Unexpected("Il silinemedi.", ex, "Il.Delete.Failed"));
            }
        }

        public async Task<Result<bool>> AnyAsync(Expression<Func<Il, bool>> predicate, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var any = await db.Il_Table.AsNoTracking().AnyAsync(predicate, cancellationToken);
                return Result.Success(any);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.AnyAsync");
                return Result.Failure<bool>(Error.Unexpected("Il sorgusu basarisiz.", ex, "Il.Any.Failed"));
            }
        }

        public async Task<Result<List<Il>>> GetAktifIllerAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.Il_Table
                    .AsNoTracking()
                    .Where(i => i.IlAdi != null
                                && !_excludedIlAdlari.Contains(i.IlAdi)
                                && i.Aktif == true)
                    .ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetAktifIllerAsync");
                return Result.Failure<List<Il>>(Error.Unexpected("Aktif iller getirilemedi.", ex, "Il.GetAktif.Failed"));
            }
        }
    }
}
