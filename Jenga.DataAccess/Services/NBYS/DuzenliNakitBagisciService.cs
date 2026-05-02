using Jenga.DataAccess.Data;
using Jenga.Models.NBYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.NBYS
{
    public class DuzenliNakitBagisciService : IDuzenliNakitBagisciService
    {
        private const string Source = nameof(DuzenliNakitBagisciService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public DuzenliNakitBagisciService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public async Task<Result<List<DuzenliNakitBagisci>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.DuzenliNakitBagisci_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result<List<DuzenliNakitBagisci>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetAllAsync hata.", ex);
                return Result<List<DuzenliNakitBagisci>>.Failure(Error.Unexpected("Düzenli nakit bağışçı listesi alınamadı.", ex));
            }
        }

        public async Task<Result<DuzenliNakitBagisci>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.DuzenliNakitBagisci_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                return entity is null
                    ? Result<DuzenliNakitBagisci>.Failure(Error.NotFound("Düzenli nakit bağışçı bulunamadı."))
                    : Result<DuzenliNakitBagisci>.Success(entity);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetByIdAsync hata.", ex);
                return Result<DuzenliNakitBagisci>.Failure(Error.Unexpected("Düzenli nakit bağışçı alınamadı.", ex));
            }
        }

        public async Task<Result> AddAsync(DuzenliNakitBagisci model, CancellationToken cancellationToken = default)
        {
            if (model is null)
                return Result.Failure(Error.Validation("Düzenli nakit bağışçı bilgisi boş olamaz."));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.DuzenliNakitBagisci_Table.AddAsync(model, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.AddAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Düzenli nakit bağışçı eklenemedi.", ex));
            }
        }

        public async Task<Result> UpdateAsync(DuzenliNakitBagisci model, CancellationToken cancellationToken = default)
        {
            if (model is null)
                return Result.Failure(Error.Validation("Düzenli nakit bağışçı bilgisi boş olamaz."));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.DuzenliNakitBagisci_Table.Update(model);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.UpdateAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Düzenli nakit bağışçı güncellenemedi.", ex));
            }
        }

        public async Task<Result> DeleteAsync(DuzenliNakitBagisci model, CancellationToken cancellationToken = default)
        {
            if (model is null)
                return Result.Failure(Error.Validation("Düzenli nakit bağışçı bilgisi boş olamaz."));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.DuzenliNakitBagisci_Table.Remove(model);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.DeleteAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Düzenli nakit bağışçı silinemedi.", ex));
            }
        }
    }
}
