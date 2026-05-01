using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public class DereceKademeDegisimService : IDereceKademeDegisimService
{
    private const string Source = nameof(DereceKademeDegisimService);
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILogService _logService;

    public DereceKademeDegisimService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
    }

    public async Task<Result<List<DereceKademeDegisim>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var list = await db.DereceKademeDegisim_Table.AsNoTracking().ToListAsync(cancellationToken);
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetAllAsync");
            return Result.Failure<List<DereceKademeDegisim>>(Error.Unexpected("Derece/kademe değişimleri getirilemedi.", ex, "DereceKademeDegisim.GetAll.Failed"));
        }
    }

    public async Task<Result<List<DereceKademeDegisim>>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var list = await db.DereceKademeDegisim_Table.AsNoTracking().Where(x => x.PersonelId == personelId).ToListAsync(cancellationToken);
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetByPersonelIdAsync");
            return Result.Failure<List<DereceKademeDegisim>>(Error.Unexpected("Derece/kademe değişimleri getirilemedi.", ex, "DereceKademeDegisim.GetByPersonelId.Failed"));
        }
    }

    public async Task<Result<DereceKademeDegisim>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.DereceKademeDegisim_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity is null)
                return Result.Failure<DereceKademeDegisim>(Error.NotFound($"Derece/kademe kaydı bulunamadı (Id={id}).", "DereceKademeDegisim.NotFound"));
            return Result.Success(entity);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetByIdAsync");
            return Result.Failure<DereceKademeDegisim>(Error.Unexpected("Derece/kademe kaydı getirilemedi.", ex, "DereceKademeDegisim.GetById.Failed"));
        }
    }

    public async Task<Result> AddAsync(DereceKademeDegisim entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Derece/kademe kaydı boş olamaz.", "DereceKademeDegisim.Null"));
        try
        {
            entity.Olusturan = modifiedBy;
            entity.OlusturmaTarihi = DateTime.Now;
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await db.DereceKademeDegisim_Table.AddAsync(entity, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.AddAsync");
            return Result.Failure(Error.Unexpected("Derece/kademe kaydı eklenemedi.", ex, "DereceKademeDegisim.Add.Failed"));
        }
    }

    public async Task<Result> UpdateAsync(DereceKademeDegisim entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Derece/kademe kaydı boş olamaz.", "DereceKademeDegisim.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var existing = await db.DereceKademeDegisim_Table.FirstOrDefaultAsync(x => x.Id == entity.Id, cancellationToken);
            if (existing is null)
                return Result.Failure(Error.NotFound("Kayıt bulunamadı!", "DereceKademeDegisim.NotFound"));
            existing.PersonelId = entity.PersonelId;
            existing.Degisim = entity.Degisim;
            existing.DegisimTarihi = entity.DegisimTarihi;
            existing.Derece = entity.Derece;
            existing.Kademe = entity.Kademe;
            existing.Aciklama = entity.Aciklama;
            existing.Degistiren = entity.Degistiren;
            existing.DegistirmeTarihi = DateTime.Now;
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.UpdateAsync");
            return Result.Failure(Error.Unexpected("Derece/kademe kaydı güncellenemedi.", ex, "DereceKademeDegisim.Update.Failed"));
        }
    }

    public async Task<Result> DeleteAsync(DereceKademeDegisim entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Derece/kademe kaydı boş olamaz.", "DereceKademeDegisim.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.DereceKademeDegisim_Table.Attach(entity);
            db.DereceKademeDegisim_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.DeleteAsync");
            return Result.Failure(Error.Unexpected("Derece/kademe kaydı silinemedi.", ex, "DereceKademeDegisim.Delete.Failed"));
        }
    }

    public async Task<Result<List<DereceKademeDegisim>>> GetDereceYukseltmeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var list = await db.DereceKademeDegisim_Table.AsNoTracking()
                .Where(x => x.Degisim == "Derece Yükseltme")
                .ToListAsync(cancellationToken);
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetDereceYukseltmeAsync");
            return Result.Failure<List<DereceKademeDegisim>>(Error.Unexpected("Derece yükseltme kayıtları getirilemedi.", ex, "DereceKademeDegisim.GetDereceYukseltme.Failed"));
        }
    }
}
