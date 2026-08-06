using Jenga.DataAccess.Data;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public class OdemePlaniService : IOdemePlaniService
    {
        private const string Source = nameof(OdemePlaniService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public OdemePlaniService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public async Task<Result<List<OdemePlani>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.OdemePlani_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result<List<OdemePlani>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetAllAsync hata.", ex);
                return Result<List<OdemePlani>>.Failure(Error.Unexpected("Ödeme planı listesi alınamadı.", ex));
            }
        }

        public async Task<Result<List<OdemePlani>>> GetAllBySozlesmeIdsAsync(IEnumerable<int> sozlesmeIds, CancellationToken cancellationToken = default)
        {
            try
            {
                var idList = sozlesmeIds.ToList();
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.OdemePlani_Table
                    .AsNoTracking()
                    .Where(p => p.SozlesmeId.HasValue && idList.Contains(p.SozlesmeId.Value))
                    .ToListAsync(cancellationToken);
                return Result<List<OdemePlani>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetAllBySozlesmeIdsAsync hata.", ex);
                return Result<List<OdemePlani>>.Failure(Error.Unexpected("Ödeme planı listesi alınamadı.", ex));
            }
        }

        public async Task<Result<List<OdemePlaniDashboardItem>>> GetAllForDashboardBySozlesmeIdsAsync(IEnumerable<int> sozlesmeIds, CancellationToken cancellationToken = default)
        {
            try
            {
                var idList = sozlesmeIds.ToList();
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.OdemePlani_Table
                    .AsNoTracking()
                    .Where(p => p.SozlesmeId.HasValue && idList.Contains(p.SozlesmeId.Value))
                    .Select(p => new OdemePlaniDashboardItem
                    {
                        SozlesmeId = p.SozlesmeId,
                        FaizliBakiye = p.FaizliBakiye,
                        VadeBitTar = p.VadeBitTar,
                        Sira = p.Sira
                    })
                    .ToListAsync(cancellationToken);
                return Result<List<OdemePlaniDashboardItem>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetAllForDashboardBySozlesmeIdsAsync hata.", ex);
                return Result<List<OdemePlaniDashboardItem>>.Failure(Error.Unexpected("Ödeme planı listesi alınamadı.", ex));
            }
        }

        public async Task<Result<OdemePlani>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.OdemePlani_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                return entity is null
                    ? Result<OdemePlani>.Failure(Error.NotFound("Ödeme planı bulunamadı."))
                    : Result<OdemePlani>.Success(entity);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetByIdAsync hata.", ex);
                return Result<OdemePlani>.Failure(Error.Unexpected("Ödeme planı alınamadı.", ex));
            }
        }

        public async Task<Result> AddAsync(OdemePlani odemePlani, CancellationToken cancellationToken = default)
        {
            if (odemePlani is null)
                return Result.Failure(Error.Validation("Ödeme planı kaydı boş olamaz."));

            if (!odemePlani.SozlesmeId.HasValue)
                return Result.Failure(Error.Validation("SozlesmeId gerekli."));

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.OdemePlani_Table.AddAsync(odemePlani, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.AddAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Ödeme planı eklenemedi.", ex));
            }
        }

        public async Task<Result> UpdateAsync(OdemePlani odemePlani, CancellationToken cancellationToken = default)
        {
            if (odemePlani is null)
                return Result.Failure(Error.Validation("Ödeme planı kaydı boş olamaz."));

            if (!odemePlani.SozlesmeId.HasValue)
                return Result.Failure(Error.Validation("SozlesmeId gerekli."));

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.OdemePlani_Table.Update(odemePlani);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.UpdateAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Ödeme planı güncellenemedi.", ex));
            }
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.OdemePlani_Table.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (entity is null)
                    return Result.Failure(Error.NotFound("Silinecek ödeme planı bulunamadı."));

                db.OdemePlani_Table.Remove(entity);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.DeleteAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Ödeme planı silinemedi.", ex));
            }
        }

        public async Task<Result<bool>> AnyAsync(Expression<Func<OdemePlani, bool>> predicate, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var any = await db.OdemePlani_Table.AnyAsync(predicate, cancellationToken);
                return Result<bool>.Success(any);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.AnyAsync hata.", ex);
                return Result<bool>.Failure(Error.Unexpected("Ödeme planı sorgulanamadı.", ex));
            }
        }
    }
}
