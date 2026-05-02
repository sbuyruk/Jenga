using Jenga.DataAccess.Data;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public class KiraSozlesmeService : IKiraSozlesmeService
    {
        private const string Source = nameof(KiraSozlesmeService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public KiraSozlesmeService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public async Task<Result<List<KiraSozlesme>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.KiraSozlesme_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result<List<KiraSozlesme>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetAllAsync hata.", ex);
                return Result<List<KiraSozlesme>>.Failure(Error.Unexpected("Kira sözleşme listesi alınamadı.", ex));
            }
        }

        public async Task<Result<KiraSozlesme>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.KiraSozlesme_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                return entity is null
                    ? Result<KiraSozlesme>.Failure(Error.NotFound("Kira sözleşmesi bulunamadı."))
                    : Result<KiraSozlesme>.Success(entity);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetByIdAsync hata.", ex);
                return Result<KiraSozlesme>.Failure(Error.Unexpected("Kira sözleşmesi alınamadı.", ex));
            }
        }

        public async Task<Result> AddAsync(KiraSozlesme sozlesme, CancellationToken cancellationToken = default)
        {
            if (sozlesme is null)
                return Result.Failure(Error.Validation("Kira sözleşmesi kaydı boş olamaz."));

            var hasParty = sozlesme.KiraciId.HasValue || sozlesme.SozBasTar.HasValue || !string.IsNullOrWhiteSpace(sozlesme.SozlesmeDurumu);
            if (!hasParty)
                return Result.Failure(Error.Validation("KiraciId, SozBasTar veya SozlesmeDurumu gerekli."));

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.KiraSozlesme_Table.AddAsync(sozlesme, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.AddAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Kira sözleşmesi eklenemedi.", ex));
            }
        }

        public async Task<Result> UpdateAsync(KiraSozlesme sozlesme, CancellationToken cancellationToken = default)
        {
            if (sozlesme is null)
                return Result.Failure(Error.Validation("Kira sözleşmesi kaydı boş olamaz."));

            var hasParty = sozlesme.KiraciId.HasValue || sozlesme.SozBasTar.HasValue || !string.IsNullOrWhiteSpace(sozlesme.SozlesmeDurumu);
            if (!hasParty)
                return Result.Failure(Error.Validation("KiraciId, SozBasTar veya SozlesmeDurumu gerekli."));

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.KiraSozlesme_Table.Update(sozlesme);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.UpdateAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Kira sözleşmesi güncellenemedi.", ex));
            }
        }

        public async Task<Result> DeleteAsync(int sozlesmeId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.KiraSozlesme_Table.FirstOrDefaultAsync(x => x.Id == sozlesmeId, cancellationToken);
                if (entity is null)
                    return Result.Failure(Error.NotFound("Silinecek kira sözleşmesi bulunamadı."));

                db.KiraSozlesme_Table.Remove(entity);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.DeleteAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Kira sözleşmesi silinemedi.", ex));
            }
        }

        public async Task<Result<bool>> AnyAsync(Expression<Func<KiraSozlesme, bool>> predicate, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var any = await db.KiraSozlesme_Table.AnyAsync(predicate, cancellationToken);
                return Result<bool>.Success(any);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.AnyAsync hata.", ex);
                return Result<bool>.Failure(Error.Unexpected("Kira sözleşmesi sorgulanamadı.", ex));
            }
        }
    }
}
