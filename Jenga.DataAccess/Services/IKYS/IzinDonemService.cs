using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public class IzinDonemService : IIzinDonemService
{
    private const string Source = nameof(IzinDonemService);
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILogService _logService;

    public IzinDonemService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
    }

    public async Task<Result<List<IzinDonem>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var list = await db.IzinDonem_Table.AsNoTracking().ToListAsync(cancellationToken);
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetAllAsync");
            return Result.Failure<List<IzinDonem>>(Error.Unexpected("İzin dönemleri getirilemedi.", ex, "IzinDonem.GetAll.Failed"));
        }
    }

    public async Task<Result<List<IzinDonem>>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var list = await db.IzinDonem_Table.AsNoTracking().Where(x => x.PersonelId == personelId).ToListAsync(cancellationToken);
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetByPersonelIdAsync");
            return Result.Failure<List<IzinDonem>>(Error.Unexpected("İzin dönemleri getirilemedi.", ex, "IzinDonem.GetByPersonelId.Failed"));
        }
    }

    public async Task<Result<IzinDonem>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.IzinDonem_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity is null)
                return Result.Failure<IzinDonem>(Error.NotFound($"İzin dönemi bulunamadı (Id={id}).", "IzinDonem.NotFound"));
            return Result.Success(entity);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetByIdAsync");
            return Result.Failure<IzinDonem>(Error.Unexpected("İzin dönemi getirilemedi.", ex, "IzinDonem.GetById.Failed"));
        }
    }

    public async Task<Result> AddAsync(IzinDonem entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("İzin dönemi boş olamaz.", "IzinDonem.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.SetCurrentUser(modifiedBy);
            await db.IzinDonem_Table.AddAsync(entity, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.AddAsync");
            return Result.Failure(Error.Unexpected("İzin dönemi eklenemedi.", ex, "IzinDonem.Add.Failed"));
        }
    }

    public async Task<Result> UpdateAsync(IzinDonem entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("İzin dönemi boş olamaz.", "IzinDonem.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var existing = await db.IzinDonem_Table.FirstOrDefaultAsync(x => x.Id == entity.Id, cancellationToken);
            if (existing is null)
                return Result.Failure(Error.NotFound("Kayıt bulunamadı!", "IzinDonem.NotFound"));
            existing.PersonelId = entity.PersonelId;
            existing.BaslangicTarihi = entity.BaslangicTarihi;
            existing.BitisTarihi = entity.BitisTarihi;
            existing.Adi = entity.Adi;
            existing.IzinTipi = entity.IzinTipi;
            existing.IzinHakki = entity.IzinHakki;
            existing.KullanilanIzin = entity.KullanilanIzin;
            existing.KalanIzin = entity.KalanIzin;
            existing.Birim = entity.Birim;
            existing.Aciklama = entity.Aciklama;
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.UpdateAsync");
            return Result.Failure(Error.Unexpected("İzin dönemi güncellenemedi.", ex, "IzinDonem.Update.Failed"));
        }
    }

    public async Task<Result> DeleteAsync(IzinDonem entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("İzin dönemi boş olamaz.", "IzinDonem.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.IzinDonem_Table.Attach(entity);
            db.IzinDonem_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.DeleteAsync");
            return Result.Failure(Error.Unexpected("İzin dönemi silinemedi.", ex, "IzinDonem.Delete.Failed"));
        }
    }
}
