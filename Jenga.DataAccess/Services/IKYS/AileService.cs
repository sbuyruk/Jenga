using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public class AileService : IAileService
{
    private const string Source = nameof(AileService);
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILogService _logService;

    public AileService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
    }

    public async Task<Result<List<Aile>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var list = await db.Aile_Table.AsNoTracking().ToListAsync(cancellationToken);
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetAllAsync");
            return Result.Failure<List<Aile>>(Error.Unexpected("Aile bilgileri getirilemedi.", ex, "Aile.GetAll.Failed"));
        }
    }

    public async Task<Result<List<Aile>>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var list = await db.Aile_Table.AsNoTracking().Where(x => x.PersonelId == personelId).ToListAsync(cancellationToken);
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetByPersonelIdAsync");
            return Result.Failure<List<Aile>>(Error.Unexpected("Aile bilgileri getirilemedi.", ex, "Aile.GetByPersonelId.Failed"));
        }
    }

    public async Task<Result<Aile>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.Aile_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity is null)
                return Result.Failure<Aile>(Error.NotFound($"Aile bilgisi bulunamadı (Id={id}).", "Aile.NotFound"));
            return Result.Success(entity);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetByIdAsync");
            return Result.Failure<Aile>(Error.Unexpected("Aile bilgisi getirilemedi.", ex, "Aile.GetById.Failed"));
        }
    }

    public async Task<Result> AddAsync(Aile entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Aile boş olamaz.", "Aile.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.SetCurrentUser(modifiedBy);
            await db.Aile_Table.AddAsync(entity, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.AddAsync");
            return Result.Failure(Error.Unexpected("Aile bilgisi eklenemedi.", ex, "Aile.Add.Failed"));
        }
    }

    public async Task<Result> UpdateAsync(Aile entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Aile boş olamaz.", "Aile.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var existing = await db.Aile_Table.FirstOrDefaultAsync(x => x.Id == entity.Id, cancellationToken);
            if (existing is null)
                return Result.Failure(Error.NotFound("Kayıt bulunamadı!", "Aile.NotFound"));
            existing.PersonelId = entity.PersonelId;
            existing.Adi = entity.Adi;
            existing.Soyadi = entity.Soyadi;
            existing.TcKimlikNo = entity.TcKimlikNo;
            existing.YakinlikDerecesi = entity.YakinlikDerecesi;
            existing.DogumTar = entity.DogumTar;
            existing.Tahsil = entity.Tahsil;
            existing.Okul = entity.Okul;
            existing.Telefon = entity.Telefon;
            existing.Meslek = entity.Meslek;
            existing.Aciklama = entity.Aciklama;
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.UpdateAsync");
            return Result.Failure(Error.Unexpected("Aile bilgisi güncellenemedi.", ex, "Aile.Update.Failed"));
        }
    }

    public async Task<Result> DeleteAsync(Aile entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Aile boş olamaz.", "Aile.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.Aile_Table.Attach(entity);
            db.Aile_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.DeleteAsync");
            return Result.Failure(Error.Unexpected("Aile bilgisi silinemedi.", ex, "Aile.Delete.Failed"));
        }
    }
}
