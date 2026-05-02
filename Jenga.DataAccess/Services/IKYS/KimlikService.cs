using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public class KimlikService : IKimlikService
{
    private const string Source = nameof(KimlikService);
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILogService _logService;

    public KimlikService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
    }

    public async Task<Result<List<Kimlik>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var list = await db.Kimlik_Table.AsNoTracking().ToListAsync(cancellationToken);
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetAllAsync");
            return Result.Failure<List<Kimlik>>(Error.Unexpected("Kimlik kayıtları getirilemedi.", ex, "Kimlik.GetAll.Failed"));
        }
    }

    public async Task<Result<Kimlik>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.Kimlik_Table.AsNoTracking().FirstOrDefaultAsync(x => x.PersonelId == personelId, cancellationToken);
            if (entity is null)
                return Result.Failure<Kimlik>(Error.NotFound($"Kimlik bulunamadı (PersonelId={personelId}).", "Kimlik.NotFound"));
            return Result.Success(entity);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetByPersonelIdAsync");
            return Result.Failure<Kimlik>(Error.Unexpected("Kimlik getirilemedi.", ex, "Kimlik.GetByPersonelId.Failed"));
        }
    }

    public async Task<Result<Kimlik>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.Kimlik_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity is null)
                return Result.Failure<Kimlik>(Error.NotFound($"Kimlik bulunamadı (Id={id}).", "Kimlik.NotFound"));
            return Result.Success(entity);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetByIdAsync");
            return Result.Failure<Kimlik>(Error.Unexpected("Kimlik getirilemedi.", ex, "Kimlik.GetById.Failed"));
        }
    }

    public async Task<Result> AddAsync(Kimlik entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Kimlik boş olamaz.", "Kimlik.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.SetCurrentUser(modifiedBy);
            await db.Kimlik_Table.AddAsync(entity, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.AddAsync");
            return Result.Failure(Error.Unexpected("Kimlik eklenemedi.", ex, "Kimlik.Add.Failed"));
        }
    }

    public async Task<Result> UpdateAsync(Kimlik entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Kimlik boş olamaz.", "Kimlik.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var existing = await db.Kimlik_Table.FirstOrDefaultAsync(x => x.Id == entity.Id, cancellationToken);
            if (existing is null)
                return Result.Failure(Error.NotFound("Kayıt bulunamadı!", "Kimlik.NotFound"));
            existing.PersonelId = entity.PersonelId;
            existing.TCKimlikNo = entity.TCKimlikNo;
            existing.BabaAdi = entity.BabaAdi;
            existing.AnneAdi = entity.AnneAdi;
            existing.DogumYeri = entity.DogumYeri;
            existing.DogumTar = entity.DogumTar;
            existing.MedeniHali = entity.MedeniHali;
            existing.EvlilikTar = entity.EvlilikTar;
            existing.Cinsiyet = entity.Cinsiyet;
            existing.EskiSoyadi = entity.EskiSoyadi;
            existing.KanGrubu = entity.KanGrubu;
            existing.DogumGunuKutlama = entity.DogumGunuKutlama;
            existing.EvlilikKutlama = entity.EvlilikKutlama;
            existing.Aciklama = entity.Aciklama;
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.UpdateAsync");
            return Result.Failure(Error.Unexpected("Kimlik güncellenemedi.", ex, "Kimlik.Update.Failed"));
        }
    }

    public async Task<Result> DeleteAsync(Kimlik entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Kimlik boş olamaz.", "Kimlik.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.Kimlik_Table.Attach(entity);
            db.Kimlik_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.DeleteAsync");
            return Result.Failure(Error.Unexpected("Kimlik silinemedi.", ex, "Kimlik.Delete.Failed"));
        }
    }
}
