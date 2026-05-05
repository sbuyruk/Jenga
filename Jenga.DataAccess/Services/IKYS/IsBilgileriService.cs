using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public class IsBilgileriService : IIsBilgileriService
{
    private const string Source = nameof(IsBilgileriService);
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILogService _logService;

    public IsBilgileriService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
    }

    public async Task<Result<List<IsBilgileri>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var list = await db.IsBilgileri_Table.AsNoTracking()
                .Include(ib => ib.UnvanTanim)
                .Include(ib => ib.BirimTanim)
                .ToListAsync(cancellationToken);
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetAllAsync");
            return Result.Failure<List<IsBilgileri>>(Error.Unexpected("İş bilgileri getirilemedi.", ex, "IsBilgileri.GetAll.Failed"));
        }
    }

    public async Task<Result<IsBilgileri>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.IsBilgileri_Table.AsNoTracking().FirstOrDefaultAsync(x => x.PersonelId == personelId, cancellationToken);
            if (entity is null)
                return Result.Failure<IsBilgileri>(Error.NotFound($"İş bilgisi bulunamadı (PersonelId={personelId}).", "IsBilgileri.NotFound"));
            return Result.Success(entity);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetByPersonelIdAsync");
            return Result.Failure<IsBilgileri>(Error.Unexpected("İş bilgisi getirilemedi.", ex, "IsBilgileri.GetByPersonelId.Failed"));
        }
    }

    public async Task<Result<IsBilgileri>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.IsBilgileri_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity is null)
                return Result.Failure<IsBilgileri>(Error.NotFound($"İş bilgisi bulunamadı (Id={id}).", "IsBilgileri.NotFound"));
            return Result.Success(entity);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetByIdAsync");
            return Result.Failure<IsBilgileri>(Error.Unexpected("İş bilgisi getirilemedi.", ex, "IsBilgileri.GetById.Failed"));
        }
    }

    public async Task<Result> AddAsync(IsBilgileri entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("İş bilgisi boş olamaz.", "IsBilgileri.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.SetCurrentUser(modifiedBy);
            await db.IsBilgileri_Table.AddAsync(entity, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.AddAsync");
            return Result.Failure(Error.Unexpected("İş bilgisi eklenemedi.", ex, "IsBilgileri.Add.Failed"));
        }
    }

    public async Task<Result> UpdateAsync(IsBilgileri entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("İş bilgisi boş olamaz.", "IsBilgileri.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var existing = await db.IsBilgileri_Table.FirstOrDefaultAsync(x => x.Id == entity.Id, cancellationToken);
            if (existing is null)
                return Result.Failure(Error.NotFound("Kayıt bulunamadı!", "IsBilgileri.NotFound"));
            existing.PersonelId = entity.PersonelId;
            existing.UnvanId = entity.UnvanId;
            existing.GorevId = entity.GorevId;
            existing.BirimId = entity.BirimId;
            existing.BaslamaTar = entity.BaslamaTar;
            existing.CalismaDurumu = entity.CalismaDurumu;
            existing.AyrilmaTar = entity.AyrilmaTar;
            existing.AyrilmaSebebi = entity.AyrilmaSebebi;
            existing.SGKSicilNo = entity.SGKSicilNo;
            existing.SGKBasTar = entity.SGKBasTar;
            existing.VakifOncesiPrimGunSayisi = entity.VakifOncesiPrimGunSayisi;
            existing.EmeklilikTarihi = entity.EmeklilikTarihi;
            existing.IzinDonemiBasTar = entity.IzinDonemiBasTar;
            existing.Aciklama = entity.Aciklama;
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.UpdateAsync");
            return Result.Failure(Error.Unexpected("İş bilgisi güncellenemedi.", ex, "IsBilgileri.Update.Failed"));
        }
    }

    public async Task<Result> DeleteAsync(IsBilgileri entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("İş bilgisi boş olamaz.", "IsBilgileri.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.IsBilgileri_Table.Attach(entity);
            db.IsBilgileri_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.DeleteAsync");
            return Result.Failure(Error.Unexpected("İş bilgisi silinemedi.", ex, "IsBilgileri.Delete.Failed"));
        }
    }
}
