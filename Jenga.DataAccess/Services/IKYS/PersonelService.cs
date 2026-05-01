using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.IKYS;

public class PersonelService : IPersonelService
{
    private const string Source = nameof(PersonelService);

    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILogService _logService;

    public PersonelService(
        IDbContextFactory<ApplicationDbContext> dbFactory,
        ILogService logService)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
    }

    public async Task<Result<List<Personel>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var list = await db.Personel_Table.AsNoTracking().ToListAsync(cancellationToken);
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetAllAsync");
            return Result.Failure<List<Personel>>(Error.Unexpected("Personeller getirilemedi.", ex, "Personel.GetAll.Failed"));
        }
    }

    public async Task<Result<Personel>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.Personel_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity is null)
                return Result.Failure<Personel>(Error.NotFound($"Personel bulunamadı (Id={id}).", "Personel.NotFound"));
            return Result.Success(entity);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetByIdAsync");
            return Result.Failure<Personel>(Error.Unexpected("Personel getirilemedi.", ex, "Personel.Get.Failed"));
        }
    }

    public async Task<Result> AddAsync(Personel personel, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        if (personel is null)
            return Result.Failure(Error.Validation("Personel boş olamaz.", "Personel.Null"));

        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await db.Personel_Table.AddAsync(personel, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.AddAsync");
            return Result.Failure(Error.Unexpected("Personel eklenemedi.", ex, "Personel.Add.Failed"));
        }
    }

    public async Task<Result> UpdateAsync(Personel personel, CancellationToken cancellationToken = default)
    {
        if (personel is null)
            return Result.Failure(Error.Validation("Personel boş olamaz.", "Personel.Null"));

        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var existing = await db.Personel_Table.FirstOrDefaultAsync(x => x.Id == personel.Id, cancellationToken);
            if (existing is null)
                return Result.Failure(Error.NotFound($"Güncellenecek personel bulunamadı (Id={personel.Id}).", "Personel.NotFound"));

            existing.Adi = personel.Adi;
            existing.Soyadi = personel.Soyadi;
            existing.KullaniciAdi = personel.KullaniciAdi;
            existing.Asker_sivil = personel.Asker_sivil;
            existing.Aciklama = personel.Aciklama;
            existing.SicilNo = personel.SicilNo;
            existing.Tahsili = personel.Tahsili;

            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.UpdateAsync");
            return Result.Failure(Error.Unexpected("Personel güncellenemedi.", ex, "Personel.Update.Failed"));
        }
    }

    public async Task<Result> DeleteAsync(Personel personel, CancellationToken cancellationToken = default)
    {
        if (personel is null)
            return Result.Failure(Error.Validation("Personel boş olamaz.", "Personel.Null"));

        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.Personel_Table.Attach(personel);
            db.Personel_Table.Remove(personel);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.DeleteAsync");
            return Result.Failure(Error.Unexpected("Personel silinemedi.", ex, "Personel.Delete.Failed"));
        }
    }

    public async Task<Result<bool>> AnyAsync(Expression<Func<Personel, bool>> predicate, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var any = await db.Personel_Table.AnyAsync(predicate, cancellationToken);
            return Result.Success(any);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.AnyAsync");
            return Result.Failure<bool>(Error.Unexpected("Sorgu çalıştırılamadı.", ex, "Personel.Any.Failed"));
        }
    }

    public Task<Result> UpdatePersonelAndSaveAsync(Personel personel, string? currentUserName, CancellationToken cancellationToken = default)
        => UpdateAsync(personel, cancellationToken);

    public Task<Result> DeletePersonelAndSaveAsync(Personel personel, string? currentUserName, CancellationToken cancellationToken = default)
        => DeleteAsync(personel, cancellationToken);

    public async Task<Result<List<Personel>>> GetKadroluPersonelAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var list = await db.Personel_Table.AsNoTracking()
                .Include(p => p.IsBilgileri)
                .Where(p => p.IsBilgileri != null && p.IsBilgileri.CalismaDurumu != null
                            && p.IsBilgileri.CalismaDurumu == "1" && p.Tipi == 1)
                .ToListAsync(cancellationToken);
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetKadroluPersonelAsync");
            return Result.Failure<List<Personel>>(Error.Unexpected("Kadrolu personeller getirilemedi.", ex, "Personel.GetKadrolu.Failed"));
        }
    }

    public async Task<Result<List<Personel>>> GetCalisanPersonelAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var list = await db.Personel_Table.AsNoTracking()
                .Include(p => p.IsBilgileri)
                .Where(p => p.IsBilgileri != null && p.IsBilgileri.CalismaDurumu != null
                            && p.IsBilgileri.CalismaDurumu == "1")
                .ToListAsync(cancellationToken);
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetCalisanPersonelAsync");
            return Result.Failure<List<Personel>>(Error.Unexpected("Çalışan personeller getirilemedi.", ex, "Personel.GetCalisan.Failed"));
        }
    }
}
