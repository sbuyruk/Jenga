using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public class IletisimBilgileriService : IIletisimBilgileriService
{
    private const string Source = nameof(IletisimBilgileriService);
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILogService _logService;

    public IletisimBilgileriService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
    }

    public async Task<Result<List<IletisimBilgileri>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var list = await db.IletisimBilgileri_Table.AsNoTracking().ToListAsync(cancellationToken);
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetAllAsync");
            return Result.Failure<List<IletisimBilgileri>>(Error.Unexpected("İletişim bilgileri getirilemedi.", ex, "IletisimBilgileri.GetAll.Failed"));
        }
    }

    public async Task<Result<IletisimBilgileri>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.IletisimBilgileri_Table.AsNoTracking().FirstOrDefaultAsync(x => x.PersonelId == personelId, cancellationToken);
            if (entity is null)
                return Result.Failure<IletisimBilgileri>(Error.NotFound($"İletişim bilgisi bulunamadı (PersonelId={personelId}).", "IletisimBilgileri.NotFound"));
            return Result.Success(entity);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetByPersonelIdAsync");
            return Result.Failure<IletisimBilgileri>(Error.Unexpected("İletişim bilgisi getirilemedi.", ex, "IletisimBilgileri.GetByPersonelId.Failed"));
        }
    }

    public async Task<Result<IletisimBilgileri>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.IletisimBilgileri_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity is null)
                return Result.Failure<IletisimBilgileri>(Error.NotFound($"İletişim bilgisi bulunamadı (Id={id}).", "IletisimBilgileri.NotFound"));
            return Result.Success(entity);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetByIdAsync");
            return Result.Failure<IletisimBilgileri>(Error.Unexpected("İletişim bilgisi getirilemedi.", ex, "IletisimBilgileri.GetById.Failed"));
        }
    }

    public async Task<Result> AddAsync(IletisimBilgileri entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("İletişim bilgisi boş olamaz.", "IletisimBilgileri.Null"));
        try
        {
            entity.Olusturan = modifiedBy;
            entity.OlusturmaTarihi = DateTime.Now;
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await db.IletisimBilgileri_Table.AddAsync(entity, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.AddAsync");
            return Result.Failure(Error.Unexpected("İletişim bilgisi eklenemedi.", ex, "IletisimBilgileri.Add.Failed"));
        }
    }

    public async Task<Result> UpdateAsync(IletisimBilgileri entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("İletişim bilgisi boş olamaz.", "IletisimBilgileri.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var existing = await db.IletisimBilgileri_Table.FirstOrDefaultAsync(x => x.Id == entity.Id, cancellationToken);
            if (existing is null)
                return Result.Failure(Error.NotFound("Kayıt bulunamadı!", "IletisimBilgileri.NotFound"));
            existing.PersonelId = entity.PersonelId;
            existing.Adres = entity.Adres;
            existing.Semt = entity.Semt;
            existing.Ili = entity.Ili;
            existing.Ilcesi = entity.Ilcesi;
            existing.PostaKodu = entity.PostaKodu;
            existing.DahiliTelefonu = entity.DahiliTelefonu;
            existing.EvTelefonu = entity.EvTelefonu;
            existing.CepTelefonu = entity.CepTelefonu;
            existing.CepTelefonu2 = entity.CepTelefonu2;
            existing.IntranetEPosta = entity.IntranetEPosta;
            existing.InternetEPosta = entity.InternetEPosta;
            existing.OzelEPosta = entity.OzelEPosta;
            existing.Aciklama = entity.Aciklama;
            existing.Degistiren = entity.Degistiren;
            existing.DegistirmeTarihi = DateTime.Now;
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.UpdateAsync");
            return Result.Failure(Error.Unexpected("İletişim bilgisi güncellenemedi.", ex, "IletisimBilgileri.Update.Failed"));
        }
    }

    public async Task<Result> DeleteAsync(IletisimBilgileri entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("İletişim bilgisi boş olamaz.", "IletisimBilgileri.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.IletisimBilgileri_Table.Attach(entity);
            db.IletisimBilgileri_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.DeleteAsync");
            return Result.Failure(Error.Unexpected("İletişim bilgisi silinemedi.", ex, "IletisimBilgileri.Delete.Failed"));
        }
    }
}
