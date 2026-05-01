using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public class YabanciDilService : IYabanciDilService
{
    private const string Source = nameof(YabanciDilService);
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILogService _logService;

    public YabanciDilService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
    }

    public async Task<Result<List<YabanciDil>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var list = await db.YabanciDil_Table.AsNoTracking().ToListAsync(cancellationToken);
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetAllAsync");
            return Result.Failure<List<YabanciDil>>(Error.Unexpected("Yabancı dil bilgileri getirilemedi.", ex, "YabanciDil.GetAll.Failed"));
        }
    }

    public async Task<Result<List<YabanciDil>>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var list = await db.YabanciDil_Table.AsNoTracking().Where(x => x.PersonelId == personelId).ToListAsync(cancellationToken);
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetByPersonelIdAsync");
            return Result.Failure<List<YabanciDil>>(Error.Unexpected("Yabancı dil bilgileri getirilemedi.", ex, "YabanciDil.GetByPersonelId.Failed"));
        }
    }

    public async Task<Result<YabanciDil>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.YabanciDil_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity is null)
                return Result.Failure<YabanciDil>(Error.NotFound($"Yabancı dil bulunamadı (Id={id}).", "YabanciDil.NotFound"));
            return Result.Success(entity);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetByIdAsync");
            return Result.Failure<YabanciDil>(Error.Unexpected("Yabancı dil getirilemedi.", ex, "YabanciDil.GetById.Failed"));
        }
    }

    public async Task<Result> AddAsync(YabanciDil entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Yabancı dil boş olamaz.", "YabanciDil.Null"));
        try
        {
            entity.Olusturan = modifiedBy;
            entity.OlusturmaTarihi = DateTime.Now;
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await db.YabanciDil_Table.AddAsync(entity, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.AddAsync");
            return Result.Failure(Error.Unexpected("Yabancı dil eklenemedi.", ex, "YabanciDil.Add.Failed"));
        }
    }

    public async Task<Result> UpdateAsync(YabanciDil entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Yabancı dil boş olamaz.", "YabanciDil.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var existing = await db.YabanciDil_Table.FirstOrDefaultAsync(x => x.Id == entity.Id, cancellationToken);
            if (existing is null)
                return Result.Failure(Error.NotFound("Kayıt bulunamadı!", "YabanciDil.NotFound"));
            existing.PersonelId = entity.PersonelId;
            existing.Dil = entity.Dil;
            existing.SinavAdi = entity.SinavAdi;
            existing.SinavNotu = entity.SinavNotu;
            existing.SinavTarihi = entity.SinavTarihi;
            existing.Aciklama = entity.Aciklama;
            existing.Degistiren = entity.Degistiren;
            existing.DegistirmeTarihi = DateTime.Now;
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.UpdateAsync");
            return Result.Failure(Error.Unexpected("Yabancı dil güncellenemedi.", ex, "YabanciDil.Update.Failed"));
        }
    }

    public async Task<Result> DeleteAsync(YabanciDil entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Yabancı dil boş olamaz.", "YabanciDil.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.YabanciDil_Table.Attach(entity);
            db.YabanciDil_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.DeleteAsync");
            return Result.Failure(Error.Unexpected("Yabancı dil silinemedi.", ex, "YabanciDil.Delete.Failed"));
        }
    }
}
