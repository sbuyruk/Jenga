using Jenga.DataAccess.Data;
using Jenga.Models.NBYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.NBYS
{
    public class BankaTanimService : IBankaTanimService
    {
        private const string Source = nameof(BankaTanimService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public BankaTanimService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService;
        }

        public async Task<Result<List<BankaTanim>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.BankaTanim_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result<List<BankaTanim>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.GetAllAsync hata.", ex);
                return Result<List<BankaTanim>>.Failure(Error.Unexpected("Banka tanım listesi alınamadı.", ex));
            }
        }

        public async Task<Result<BankaTanim>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.BankaTanim_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                return entity is null
                    ? Result<BankaTanim>.Failure(Error.NotFound("Banka tanım bulunamadı."))
                    : Result<BankaTanim>.Success(entity);
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.GetByIdAsync hata.", ex);
                return Result<BankaTanim>.Failure(Error.Unexpected("Banka tanım alınamadı.", ex));
            }
        }

        public async Task<Result> AddAsync(BankaTanim model, CancellationToken cancellationToken = default)
        {
            if (model is null)
                return Result.Failure(Error.Validation("Banka tanım bilgisi boş olamaz."));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.BankaTanim_Table.AddAsync(model, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.AddAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Banka tanım eklenemedi.", ex));
            }
        }

        public async Task<Result> UpdateAsync(BankaTanim model, CancellationToken cancellationToken = default)
        {
            if (model is null)
                return Result.Failure(Error.Validation("Banka tanım bilgisi boş olamaz."));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.BankaTanim_Table.Update(model);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.UpdateAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Banka tanım güncellenemedi.", ex));
            }
        }

        public async Task<Result> DeleteAsync(BankaTanim model, CancellationToken cancellationToken = default)
        {
            if (model is null)
                return Result.Failure(Error.Validation("Banka tanım bilgisi boş olamaz."));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.BankaTanim_Table.Remove(model);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService?.LogError($"{Source}.DeleteAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Banka tanım silinemedi.", ex));
            }
        }
    }
}
