using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public class GorevTanimService : IGorevTanimService
{
    private const string Source = nameof(GorevTanimService);
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILogService _logService;

    public GorevTanimService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
    }

    public async Task<Result<List<GorevTanim>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var list = await db.GorevTanim_Table.AsNoTracking().ToListAsync(cancellationToken);
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetAllAsync");
            return Result.Failure<List<GorevTanim>>(Error.Unexpected("Görev tanımları getirilemedi.", ex, "GorevTanim.GetAll.Failed"));
        }
    }

    public async Task<Result<List<GorevTanim>>> GetByBirimIdAsync(int birimId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var list = await db.GorevTanim_Table.AsNoTracking().Where(x => x.BirimId == birimId).ToListAsync(cancellationToken);
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetByBirimIdAsync");
            return Result.Failure<List<GorevTanim>>(Error.Unexpected("Görev tanımları getirilemedi.", ex, "GorevTanim.GetByBirimId.Failed"));
        }
    }

    public async Task<Result<GorevTanim>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.GorevTanim_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity is null)
                return Result.Failure<GorevTanim>(Error.NotFound($"Görev tanımı bulunamadı (Id={id}).", "GorevTanim.NotFound"));
            return Result.Success(entity);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetByIdAsync");
            return Result.Failure<GorevTanim>(Error.Unexpected("Görev tanımı getirilemedi.", ex, "GorevTanim.GetById.Failed"));
        }
    }

    public async Task<Result> AddAsync(GorevTanim entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Görev tanımı boş olamaz.", "GorevTanim.Null"));
        try
        {
            entity.Olusturan = modifiedBy;
            entity.OlusturmaTarihi = DateTime.Now;
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await db.GorevTanim_Table.AddAsync(entity, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.AddAsync");
            return Result.Failure(Error.Unexpected("Görev tanımı eklenemedi.", ex, "GorevTanim.Add.Failed"));
        }
    }

    public async Task<Result> UpdateAsync(GorevTanim entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Görev tanımı boş olamaz.", "GorevTanim.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var existing = await db.GorevTanim_Table.FirstOrDefaultAsync(x => x.Id == entity.Id, cancellationToken);
            if (existing is null)
                return Result.Failure(Error.NotFound("Kayıt bulunamadı!", "GorevTanim.NotFound"));
            existing.BirimId = entity.BirimId;
            existing.Adi = entity.Adi;
            existing.KisaAdi = entity.KisaAdi;
            existing.PersonelId = entity.PersonelId;
            existing.Vekil = entity.Vekil;
            existing.Aktif = entity.Aktif;
            existing.Aciklama = entity.Aciklama;
            existing.Degistiren = entity.Degistiren;
            existing.DegistirmeTarihi = DateTime.Now;
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.UpdateAsync");
            return Result.Failure(Error.Unexpected("Görev tanımı güncellenemedi.", ex, "GorevTanim.Update.Failed"));
        }
    }

    public async Task<Result> DeleteAsync(GorevTanim entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Görev tanımı boş olamaz.", "GorevTanim.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.GorevTanim_Table.Attach(entity);
            db.GorevTanim_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.DeleteAsync");
            return Result.Failure(Error.Unexpected("Görev tanımı silinemedi.", ex, "GorevTanim.Delete.Failed"));
        }
    }
}
