using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public class IzinHareketService : IIzinHareketService
{
    private const string Source = nameof(IzinHareketService);
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILogService _logService;

    public IzinHareketService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
    }

    public async Task<Result<List<IzinHareket>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var list = await db.IzinHareket_Table.AsNoTracking().ToListAsync(cancellationToken);
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetAllAsync");
            return Result.Failure<List<IzinHareket>>(Error.Unexpected("İzin hareketleri getirilemedi.", ex, "IzinHareket.GetAll.Failed"));
        }
    }

    public async Task<Result<List<IzinHareket>>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var list = await db.IzinHareket_Table.AsNoTracking().Where(x => x.PersonelId == personelId).ToListAsync(cancellationToken);
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetByPersonelIdAsync");
            return Result.Failure<List<IzinHareket>>(Error.Unexpected("İzin hareketleri getirilemedi.", ex, "IzinHareket.GetByPersonelId.Failed"));
        }
    }

    public async Task<Result<IzinHareket>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.IzinHareket_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity is null)
                return Result.Failure<IzinHareket>(Error.NotFound($"İzin hareketi bulunamadı (Id={id}).", "IzinHareket.NotFound"));
            return Result.Success(entity);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetByIdAsync");
            return Result.Failure<IzinHareket>(Error.Unexpected("İzin hareketi getirilemedi.", ex, "IzinHareket.GetById.Failed"));
        }
    }

    public async Task<Result> AddAsync(IzinHareket entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("İzin hareketi boş olamaz.", "IzinHareket.Null"));
        try
        {
            entity.Mahsup ??= false;
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.SetCurrentUser(modifiedBy);
            await db.IzinHareket_Table.AddAsync(entity, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.AddAsync");
            return Result.Failure(Error.Unexpected("İzin hareketi eklenemedi.", ex, "IzinHareket.Add.Failed"));
        }
    }

    public async Task<Result> UpdateAsync(IzinHareket entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("İzin hareketi boş olamaz.", "IzinHareket.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var existing = await db.IzinHareket_Table.FirstOrDefaultAsync(x => x.Id == entity.Id, cancellationToken);
            if (existing is null)
                return Result.Failure(Error.NotFound("Kayıt bulunamadı!", "IzinHareket.NotFound"));
            existing.PersonelId = entity.PersonelId;
            existing.IzinTalepId = entity.IzinTalepId;
            existing.IzinDonemId = entity.IzinDonemId;
            existing.IzinTipi = entity.IzinTipi;
            existing.BaslangicTarihi = entity.BaslangicTarihi;
            existing.BitisTarihi = entity.BitisTarihi;
            existing.Sure = entity.Sure;
            existing.Birim = entity.Birim;
            existing.Adres = entity.Adres;
            existing.VekilImza = entity.VekilImza;
            existing.AmirImza = entity.AmirImza;
            existing.OnayImza = entity.OnayImza;
            existing.Mahsup = entity.Mahsup;
            existing.OncekiIzinStr = entity.OncekiIzinStr;
            existing.KullanilanIzinStr = entity.KullanilanIzinStr;
            existing.KalanIzinStr = entity.KalanIzinStr;
            existing.Aciklama = entity.Aciklama;
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.UpdateAsync");
            return Result.Failure(Error.Unexpected("İzin hareketi güncellenemedi.", ex, "IzinHareket.Update.Failed"));
        }
    }

    public async Task<Result> DeleteAsync(IzinHareket entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("İzin hareketi boş olamaz.", "IzinHareket.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.IzinHareket_Table.Attach(entity);
            db.IzinHareket_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.DeleteAsync");
            return Result.Failure(Error.Unexpected("İzin hareketi silinemedi.", ex, "IzinHareket.Delete.Failed"));
        }
    }
}
