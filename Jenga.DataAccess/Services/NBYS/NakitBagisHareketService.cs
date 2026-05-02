using Jenga.DataAccess.Data;
using Jenga.Models.NBYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.NBYS
{
    public class NakitBagisHareketService : INakitBagisHareketService
    {
        private const string Source = nameof(NakitBagisHareketService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public NakitBagisHareketService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public async Task<Result<List<NakitBagisHareket>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.NakitBagisHareket_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result<List<NakitBagisHareket>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetAllAsync hata.", ex);
                return Result<List<NakitBagisHareket>>.Failure(Error.Unexpected("Nakit bağış hareket listesi alınamadı.", ex));
            }
        }

        public async Task<Result<List<NakitBagisHareket>>> GetLastYearsAsync(int years, CancellationToken cancellationToken = default)
        {
            try
            {
                var startDate = DateTime.Today.AddYears(-years);
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.NakitBagisHareket_Table
                    .AsNoTracking()
                    .Where(x => x.BagisTarihi != null && x.BagisTarihi.Value >= startDate)
                    .ToListAsync(cancellationToken);
                return Result<List<NakitBagisHareket>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetLastYearsAsync hata.", ex);
                return Result<List<NakitBagisHareket>>.Failure(Error.Unexpected("Nakit bağış hareket listesi alınamadı.", ex));
            }
        }

        public async Task<Result<NakitBagisHareket>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.NakitBagisHareket_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                return entity is null
                    ? Result<NakitBagisHareket>.Failure(Error.NotFound("Nakit bağış hareket bulunamadı."))
                    : Result<NakitBagisHareket>.Success(entity);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetByIdAsync hata.", ex);
                return Result<NakitBagisHareket>.Failure(Error.Unexpected("Nakit bağış hareket alınamadı.", ex));
            }
        }

        public async Task<Result> AddAsync(NakitBagisHareket model, CancellationToken cancellationToken = default)
        {
            if (model is null)
                return Result.Failure(Error.Validation("Nakit bağış hareket bilgisi boş olamaz."));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.NakitBagisHareket_Table.AddAsync(model, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.AddAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Nakit bağış hareket eklenemedi.", ex));
            }
        }

        public async Task<Result> UpdateAsync(NakitBagisHareket model, CancellationToken cancellationToken = default)
        {
            if (model is null)
                return Result.Failure(Error.Validation("Nakit bağış hareket bilgisi boş olamaz."));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.NakitBagisHareket_Table.Update(model);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.UpdateAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Nakit bağış hareket güncellenemedi.", ex));
            }
        }

        public async Task<Result> DeleteAsync(NakitBagisHareket model, CancellationToken cancellationToken = default)
        {
            if (model is null)
                return Result.Failure(Error.Validation("Nakit bağış hareket bilgisi boş olamaz."));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.NakitBagisHareket_Table.Remove(model);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.DeleteAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Nakit bağış hareket silinemedi.", ex));
            }
        }
    }
}
