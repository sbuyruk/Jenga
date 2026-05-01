using Jenga.DataAccess.Data;
using Jenga.Models.FTK;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.FTK
{
    public class FtkService : IFtkService
    {
        private const string Source = nameof(FtkService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public FtkService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService;
        }

        public async Task<Result<List<Ftk>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.FTK_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.GetAllAsync");
                return Result.Failure<List<Ftk>>(Error.Unexpected("FTK listesi alınamadı.", ex, "Ftk.GetAll.Failed"));
            }
        }

        public async Task<Result<List<Ftk>>> GetLatestPerIslemAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var ftkSet = db.FTK_Table.AsNoTracking();

                // WHERE Sayac = (SELECT MAX(Sayac) FROM FTK_Table WHERE FTKIslemId = A.FTKIslemId)
                var list = await (
                    from f in ftkSet
                    where f.FtkIslemId != null
                       && f.Sayac == ftkSet
                            .Where(x => x.FtkIslemId == f.FtkIslemId)
                            .Max(x => x.Sayac)
                    select f
                ).ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.GetLatestPerIslemAsync");
                return Result.Failure<List<Ftk>>(Error.Unexpected("FTK son kayıtları alınamadı.", ex, "Ftk.GetLatestPerIslem.Failed"));
            }
        }

        public async Task<Result<Ftk>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.FTK_Table.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
                if (entity is null)
                    return Result.Failure<Ftk>(Error.NotFound($"FTK bulunamadı (Id={id}).", "Ftk.NotFound"));
                return Result.Success(entity);
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.GetByIdAsync");
                return Result.Failure<Ftk>(Error.Unexpected("FTK getirilemedi.", ex, "Ftk.GetById.Failed"));
            }
        }

        public async Task<Result> AddAsync(Ftk model, CancellationToken cancellationToken = default)
        {
            if (model == null)
                return Result.Failure(Error.Validation("FTK boş olamaz.", "Ftk.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.FTK_Table.AddAsync(model, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.AddAsync");
                return Result.Failure(Error.Unexpected("FTK eklenemedi.", ex, "Ftk.Add.Failed"));
            }
        }

        public async Task<Result> UpdateAsync(Ftk model, CancellationToken cancellationToken = default)
        {
            if (model == null)
                return Result.Failure(Error.Validation("FTK boş olamaz.", "Ftk.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.FTK_Table.Update(model);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.UpdateAsync");
                return Result.Failure(Error.Unexpected("FTK güncellenemedi.", ex, "Ftk.Update.Failed"));
            }
        }

        public async Task<Result> DeleteAsync(Ftk model, CancellationToken cancellationToken = default)
        {
            if (model == null)
                return Result.Failure(Error.Validation("FTK boş olamaz.", "Ftk.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.FTK_Table.Remove(model);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.DeleteAsync");
                return Result.Failure(Error.Unexpected("FTK silinemedi.", ex, "Ftk.Delete.Failed"));
            }
        }
    }
}
