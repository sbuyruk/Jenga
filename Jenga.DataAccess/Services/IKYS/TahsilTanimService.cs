using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public class TahsilTanimService : ITahsilTanimService
{
    private const string Source = nameof(TahsilTanimService);
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILogService _logService;

    public TahsilTanimService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
    }

    public async Task<Result<List<TahsilTanim>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var list = await db.TahsilTanim_Table.AsNoTracking().ToListAsync(cancellationToken);
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetAllAsync");
            return Result.Failure<List<TahsilTanim>>(Error.Unexpected("Tahsil tanımları getirilemedi.", ex, "TahsilTanim.GetAll.Failed"));
        }
    }

    public async Task<Result<TahsilTanim>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.TahsilTanim_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity is null)
                return Result.Failure<TahsilTanim>(Error.NotFound($"Tahsil tanımı bulunamadı (Id={id}).", "TahsilTanim.NotFound"));
            return Result.Success(entity);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetByIdAsync");
            return Result.Failure<TahsilTanim>(Error.Unexpected("Tahsil tanımı getirilemedi.", ex, "TahsilTanim.GetById.Failed"));
        }
    }

    public async Task<Result> AddAsync(TahsilTanim entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Tahsil tanımı boş olamaz.", "TahsilTanim.Null"));
        try
        {
            entity.Olusturan = modifiedBy;
            entity.OlusturmaTarihi = DateTime.Now;
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await db.TahsilTanim_Table.AddAsync(entity, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.AddAsync");
            return Result.Failure(Error.Unexpected("Tahsil tanımı eklenemedi.", ex, "TahsilTanim.Add.Failed"));
        }
    }

    public async Task<Result> UpdateAsync(TahsilTanim entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Tahsil tanımı boş olamaz.", "TahsilTanim.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var existing = await db.TahsilTanim_Table.FirstOrDefaultAsync(x => x.Id == entity.Id, cancellationToken);
            if (existing is null)
                return Result.Failure(Error.NotFound("Kayıt bulunamadı!", "TahsilTanim.NotFound"));
            existing.TahsilDurumu = entity.TahsilDurumu;
            existing.Degistiren = entity.Degistiren;
            existing.DegistirmeTarihi = DateTime.Now;
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.UpdateAsync");
            return Result.Failure(Error.Unexpected("Tahsil tanımı güncellenemedi.", ex, "TahsilTanim.Update.Failed"));
        }
    }

    public async Task<Result> DeleteAsync(TahsilTanim entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Tahsil tanımı boş olamaz.", "TahsilTanim.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.TahsilTanim_Table.Attach(entity);
            db.TahsilTanim_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.DeleteAsync");
            return Result.Failure(Error.Unexpected("Tahsil tanımı silinemedi.", ex, "TahsilTanim.Delete.Failed"));
        }
    }
}
