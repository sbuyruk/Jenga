using Jenga.DataAccess.Data;
using Jenga.Models.NBYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.NBYS
{
    public class NakitBagisciService : INakitBagisciService
    {
        private const string Source = nameof(NakitBagisciService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public NakitBagisciService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public async Task<Result<List<NakitBagisci>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.NakitBagisci_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result<List<NakitBagisci>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetAllAsync hata.", ex);
                return Result<List<NakitBagisci>>.Failure(Error.Unexpected("Nakit bağışçı listesi alınamadı.", ex));
            }
        }

        public async Task<Result<NakitBagisci>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.NakitBagisci_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                return entity is null
                    ? Result<NakitBagisci>.Failure(Error.NotFound("Nakit bağışçı bulunamadı."))
                    : Result<NakitBagisci>.Success(entity);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetByIdAsync hata.", ex);
                return Result<NakitBagisci>.Failure(Error.Unexpected("Nakit bağışçı alınamadı.", ex));
            }
        }

        public async Task<Result> AddAsync(NakitBagisci model, CancellationToken cancellationToken = default)
        {
            if (model is null)
                return Result.Failure(Error.Validation("Nakit bağışçı bilgisi boş olamaz."));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.NakitBagisci_Table.AddAsync(model, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.AddAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Nakit bağışçı eklenemedi.", ex));
            }
        }

        public async Task<Result> UpdateAsync(NakitBagisci model, CancellationToken cancellationToken = default)
        {
            if (model is null)
                return Result.Failure(Error.Validation("Nakit bağışçı bilgisi boş olamaz."));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.NakitBagisci_Table.Update(model);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.UpdateAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Nakit bağışçı güncellenemedi.", ex));
            }
        }

        public async Task<Result> DeleteAsync(NakitBagisci model, CancellationToken cancellationToken = default)
        {
            if (model is null)
                return Result.Failure(Error.Validation("Nakit bağışçı bilgisi boş olamaz."));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.NakitBagisci_Table.Remove(model);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.DeleteAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Nakit bağışçı silinemedi.", ex));
            }
        }
    }
}
