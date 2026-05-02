using Jenga.DataAccess.Data;
using Jenga.Models.NBYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.NBYS
{
    public class ArmaganTanimService : IArmaganTanimService
    {
        private const string Source = nameof(ArmaganTanimService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public ArmaganTanimService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public async Task<Result<List<ArmaganTanim>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.ArmaganTanim_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result<List<ArmaganTanim>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetAllAsync hata.", ex);
                return Result<List<ArmaganTanim>>.Failure(Error.Unexpected("Armağan tanım listesi alınamadı.", ex));
            }
        }

        public async Task<Result<ArmaganTanim>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.ArmaganTanim_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                return entity is null
                    ? Result<ArmaganTanim>.Failure(Error.NotFound("Armağan tanım bulunamadı."))
                    : Result<ArmaganTanim>.Success(entity);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetByIdAsync hata.", ex);
                return Result<ArmaganTanim>.Failure(Error.Unexpected("Armağan tanım alınamadı.", ex));
            }
        }

        public async Task<Result> AddAsync(ArmaganTanim model, CancellationToken cancellationToken = default)
        {
            if (model is null)
                return Result.Failure(Error.Validation("Armağan tanım bilgisi boş olamaz."));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.ArmaganTanim_Table.AddAsync(model, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.AddAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Armağan tanım eklenemedi.", ex));
            }
        }

        public async Task<Result> UpdateAsync(ArmaganTanim model, CancellationToken cancellationToken = default)
        {
            if (model is null)
                return Result.Failure(Error.Validation("Armağan tanım bilgisi boş olamaz."));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.ArmaganTanim_Table.Update(model);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.UpdateAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Armağan tanım güncellenemedi.", ex));
            }
        }

        public async Task<Result> DeleteAsync(ArmaganTanim model, CancellationToken cancellationToken = default)
        {
            if (model is null)
                return Result.Failure(Error.Validation("Armağan tanım bilgisi boş olamaz."));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.ArmaganTanim_Table.Remove(model);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.DeleteAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Armağan tanım silinemedi.", ex));
            }
        }
    }
}
