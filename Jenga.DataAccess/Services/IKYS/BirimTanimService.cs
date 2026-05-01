using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public class BirimTanimService : IBirimTanimService
{
    private const string Source = nameof(BirimTanimService);
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILogService _logService;

    public BirimTanimService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
    }

    public async Task<Result<List<BirimTanim>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var list = await db.BirimTanim_Table.AsNoTracking().ToListAsync(cancellationToken);
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetAllAsync");
            return Result.Failure<List<BirimTanim>>(Error.Unexpected("Birim tanımları getirilemedi.", ex, "BirimTanim.GetAll.Failed"));
        }
    }

    public async Task<Result<BirimTanim>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.BirimTanim_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity is null)
                return Result.Failure<BirimTanim>(Error.NotFound($"Birim tanımı bulunamadı (Id={id}).", "BirimTanim.NotFound"));
            return Result.Success(entity);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetByIdAsync");
            return Result.Failure<BirimTanim>(Error.Unexpected("Birim tanımı getirilemedi.", ex, "BirimTanim.GetById.Failed"));
        }
    }

    public async Task<Result> AddAsync(BirimTanim entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Birim tanımı boş olamaz.", "BirimTanim.Null"));
        try
        {
            entity.Olusturan = modifiedBy;
            entity.OlusturmaTarihi = DateTime.Now;
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await db.BirimTanim_Table.AddAsync(entity, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.AddAsync");
            return Result.Failure(Error.Unexpected("Birim tanımı eklenemedi.", ex, "BirimTanim.Add.Failed"));
        }
    }

    public async Task<Result> UpdateAsync(BirimTanim entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Birim tanımı boş olamaz.", "BirimTanim.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var existing = await db.BirimTanim_Table.FirstOrDefaultAsync(x => x.Id == entity.Id, cancellationToken);
            if (existing is null)
                return Result.Failure(Error.NotFound("Kayıt bulunamadı!", "BirimTanim.NotFound"));
            existing.Adi = entity.Adi;
            existing.KisaAdi = entity.KisaAdi;
            existing.ParentId = entity.ParentId;
            existing.AmirId = entity.AmirId;
            existing.Sira = entity.Sira;
            existing.Aktif = entity.Aktif;
            existing.BolgeId = entity.BolgeId;
            existing.Aciklama = entity.Aciklama;
            existing.Degistiren = entity.Degistiren;
            existing.DegistirmeTarihi = DateTime.Now;
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.UpdateAsync");
            return Result.Failure(Error.Unexpected("Birim tanımı güncellenemedi.", ex, "BirimTanim.Update.Failed"));
        }
    }

    public async Task<Result> DeleteAsync(BirimTanim entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Birim tanımı boş olamaz.", "BirimTanim.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.BirimTanim_Table.Attach(entity);
            db.BirimTanim_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.DeleteAsync");
            return Result.Failure(Error.Unexpected("Birim tanımı silinemedi.", ex, "BirimTanim.Delete.Failed"));
        }
    }
}
