using Jenga.DataAccess.Data;
using Jenga.Models.NBYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.NBYS
{
    public class ArmaganService : IArmaganService
    {
        private const string Source = nameof(ArmaganService);

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public ArmaganService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public async Task<Result<List<Armagan>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.Armagan_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result<List<Armagan>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetAllAsync hata.", ex);
                return Result<List<Armagan>>.Failure(Error.Unexpected("Armağan listesi alınamadı.", ex));
            }
        }

        public async Task<Result<List<ArmaganDashboardItem>>> GetAllForDashboardAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.Armagan_Table
                    .AsNoTracking()
                    .Where(x => x.Tarih != null && x.Tarih.Value >= new DateTime(2005, 1, 1))
                    .Select(x => new ArmaganDashboardItem
                    {
                        BagisciId       = x.BagisciId,
                        ArmaganTanimId  = x.ArmaganTanimId,
                        Tarih           = x.Tarih,
                        Durum           = x.Durum,
                        BagisMiktari    = x.BagisMiktari,
                        DovizCinsi      = x.DovizCinsi,
                        BelgedeYazanIsim = x.BelgedeYazanIsim,
                        DuzenliBagis    = x.DuzenliBagis,
                        CokluBagis      = x.CokluBagis
                    })
                    .ToListAsync(cancellationToken);
                return Result<List<ArmaganDashboardItem>>.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetAllForDashboardAsync hata.", ex);
                return Result<List<ArmaganDashboardItem>>.Failure(Error.Unexpected("Armağan listesi alınamadı.", ex));
            }
        }

        public async Task<Result<Armagan>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.Armagan_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                return entity is null
                    ? Result<Armagan>.Failure(Error.NotFound("Armağan bulunamadı."))
                    : Result<Armagan>.Success(entity);
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.GetByIdAsync hata.", ex);
                return Result<Armagan>.Failure(Error.Unexpected("Armağan alınamadı.", ex));
            }
        }

        public async Task<Result> AddAsync(Armagan model, CancellationToken cancellationToken = default)
        {
            if (model is null)
                return Result.Failure(Error.Validation("Armağan bilgisi boş olamaz."));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.Armagan_Table.AddAsync(model, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.AddAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Armağan eklenemedi.", ex));
            }
        }

        public async Task<Result> UpdateAsync(Armagan model, CancellationToken cancellationToken = default)
        {
            if (model is null)
                return Result.Failure(Error.Validation("Armağan bilgisi boş olamaz."));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.Armagan_Table.Update(model);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.UpdateAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Armağan güncellenemedi.", ex));
            }
        }

        public async Task<Result> DeleteAsync(Armagan model, CancellationToken cancellationToken = default)
        {
            if (model is null)
                return Result.Failure(Error.Validation("Armağan bilgisi boş olamaz."));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.Armagan_Table.Remove(model);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogError($"{Source}.DeleteAsync hata.", ex);
                return Result.Failure(Error.Unexpected("Armağan silinemedi.", ex));
            }
        }
    }
}
