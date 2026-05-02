using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public class EgitimSeviyesiService : IEgitimSeviyesiService
{
    private const string Source = nameof(EgitimSeviyesiService);
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILogService _logService;

    public EgitimSeviyesiService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
    }

    public async Task<Result<List<EgitimSeviyesi>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var list = await db.EgitimSeviyesi_Table.AsNoTracking().ToListAsync(cancellationToken);
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetAllAsync");
            return Result.Failure<List<EgitimSeviyesi>>(Error.Unexpected("Eğitim seviyeleri getirilemedi.", ex, "EgitimSeviyesi.GetAll.Failed"));
        }
    }

    public async Task<Result<EgitimSeviyesi>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.EgitimSeviyesi_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity is null)
                return Result.Failure<EgitimSeviyesi>(Error.NotFound($"Eğitim seviyesi bulunamadı (Id={id}).", "EgitimSeviyesi.NotFound"));
            return Result.Success(entity);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetByIdAsync");
            return Result.Failure<EgitimSeviyesi>(Error.Unexpected("Eğitim seviyesi getirilemedi.", ex, "EgitimSeviyesi.GetById.Failed"));
        }
    }

    public async Task<Result> AddAsync(EgitimSeviyesi entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Eğitim seviyesi boş olamaz.", "EgitimSeviyesi.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.SetCurrentUser(modifiedBy);
            await db.EgitimSeviyesi_Table.AddAsync(entity, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.AddAsync");
            return Result.Failure(Error.Unexpected("Eğitim seviyesi eklenemedi.", ex, "EgitimSeviyesi.Add.Failed"));
        }
    }

    public async Task<Result> UpdateAsync(EgitimSeviyesi entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Eğitim seviyesi boş olamaz.", "EgitimSeviyesi.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var existing = await db.EgitimSeviyesi_Table.FirstOrDefaultAsync(x => x.Id == entity.Id, cancellationToken);
            if (existing is null)
                return Result.Failure(Error.NotFound("Kayıt bulunamadı!", "EgitimSeviyesi.NotFound"));
            existing.Adi = entity.Adi;
            existing.KisaAdi = entity.KisaAdi;
            existing.Aciklama = entity.Aciklama;
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.UpdateAsync");
            return Result.Failure(Error.Unexpected("Eğitim seviyesi güncellenemedi.", ex, "EgitimSeviyesi.Update.Failed"));
        }
    }

    public async Task<Result> DeleteAsync(EgitimSeviyesi entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Eğitim seviyesi boş olamaz.", "EgitimSeviyesi.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.EgitimSeviyesi_Table.Attach(entity);
            db.EgitimSeviyesi_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.DeleteAsync");
            return Result.Failure(Error.Unexpected("Eğitim seviyesi silinemedi.", ex, "EgitimSeviyesi.Delete.Failed"));
        }
    }
}
