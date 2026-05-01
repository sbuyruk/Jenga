using Jenga.DataAccess.Data;
using Jenga.Models.FTK;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.FTK
{
    public class FtkKisiService : IFtkKisiService
    {
        private const string Source = nameof(FtkKisiService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public FtkKisiService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService;
        }

        public async Task<Result<List<FtkKisi>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.FTKKisi_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.GetAllAsync");
                return Result.Failure<List<FtkKisi>>(Error.Unexpected("FTK kişi listesi alınamadı.", ex, "FtkKisi.GetAll.Failed"));
            }
        }

        public async Task<Result<FtkKisi>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.FTKKisi_Table.AsNoTracking().FirstOrDefaultAsync(k => k.Id == id, cancellationToken);
                if (entity is null)
                    return Result.Failure<FtkKisi>(Error.NotFound($"FTK kişi bulunamadı (Id={id}).", "FtkKisi.NotFound"));
                return Result.Success(entity);
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.GetByIdAsync");
                return Result.Failure<FtkKisi>(Error.Unexpected("FTK kişi getirilemedi.", ex, "FtkKisi.GetById.Failed"));
            }
        }

        public async Task<Result> AddAsync(FtkKisi model, CancellationToken cancellationToken = default)
        {
            if (model == null)
                return Result.Failure(Error.Validation("FTK kişi boş olamaz.", "FtkKisi.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.FTKKisi_Table.AddAsync(model, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.AddAsync");
                return Result.Failure(Error.Unexpected("FTK kişi eklenemedi.", ex, "FtkKisi.Add.Failed"));
            }
        }

        public async Task<Result> UpdateAsync(FtkKisi model, CancellationToken cancellationToken = default)
        {
            if (model == null)
                return Result.Failure(Error.Validation("FTK kişi boş olamaz.", "FtkKisi.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.FTKKisi_Table.Update(model);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.UpdateAsync");
                return Result.Failure(Error.Unexpected("FTK kişi güncellenemedi.", ex, "FtkKisi.Update.Failed"));
            }
        }

        public async Task<Result> DeleteAsync(FtkKisi model, CancellationToken cancellationToken = default)
        {
            if (model == null)
                return Result.Failure(Error.Validation("FTK kişi boş olamaz.", "FtkKisi.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.FTKKisi_Table.Remove(model);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.DeleteAsync");
                return Result.Failure(Error.Unexpected("FTK kişi silinemedi.", ex, "FtkKisi.Delete.Failed"));
            }
        }
    }
}
