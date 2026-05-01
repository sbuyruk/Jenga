using Jenga.DataAccess.Data;
using Jenga.Models.Common;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.Common
{
    public class IlceService : IIlceService
    {
        private const string Source = nameof(IlceService);
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService? _logService;
        private List<Ilce>? _cache;

        public IlceService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService? logService = null)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService;
        }

        public async Task<Result<List<Ilce>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_cache == null)
                {
                    await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                    _cache = await db.Ilce_Table.AsNoTracking().ToListAsync(cancellationToken);
                }
                return Result.Success(_cache);
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.GetAllAsync");
                return Result.Failure<List<Ilce>>(Error.Unexpected("İlçeler getirilemedi.", ex, "Ilce.GetAll.Failed"));
            }
        }

        public async Task<Result<Ilce>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.Ilce_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (entity is null)
                    return Result.Failure<Ilce>(Error.NotFound($"İlçe bulunamadı (Id={id}).", "Ilce.NotFound"));
                return Result.Success(entity);
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.GetByIdAsync");
                return Result.Failure<Ilce>(Error.Unexpected("İlçe getirilemedi.", ex, "Ilce.GetById.Failed"));
            }
        }

        public async Task<Result<List<Ilce>>> GetByIlIdAsync(int ilId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.Ilce_Table
                    .AsNoTracking()
                    .Where(x => x.IlId == ilId)
                    .ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.GetByIlIdAsync");
                return Result.Failure<List<Ilce>>(Error.Unexpected("İlçeler getirilemedi.", ex, "Ilce.GetByIlId.Failed"));
            }
        }

        public async Task<Result<List<Ilce>>> GetAktifIlcelerAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.Ilce_Table
                    .AsNoTracking()
                    .Where(i => i.IlceAdi != null && i.IlceAdi != "Merkez" && i.Aktif == true)
                    .ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService?.LogException(ex, $"{Source}.GetAktifIlcelerAsync");
                return Result.Failure<List<Ilce>>(Error.Unexpected("Aktif ilçeler getirilemedi.", ex, "Ilce.GetAktif.Failed"));
            }
        }
    }
}
