using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public class GorevOnayService : IGorevOnayService
{
    private const string Source = nameof(GorevOnayService);
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILogService _logService;

    public GorevOnayService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
    }

    public async Task<Result<List<GorevOnay>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var list = await db.GorevOnay_Table.AsNoTracking().ToListAsync(cancellationToken);
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetAllAsync");
            return Result.Failure<List<GorevOnay>>(Error.Unexpected("Görev onayları getirilemedi.", ex, "GorevOnay.GetAll.Failed"));
        }
    }

    public async Task<Result<List<GorevOnay>>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var list = await db.GorevOnay_Table.AsNoTracking().Where(x => x.PersonelId == personelId).ToListAsync(cancellationToken);
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetByPersonelIdAsync");
            return Result.Failure<List<GorevOnay>>(Error.Unexpected("Görev onayları getirilemedi.", ex, "GorevOnay.GetByPersonelId.Failed"));
        }
    }

    public async Task<Result<GorevOnay>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.GorevOnay_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity is null)
                return Result.Failure<GorevOnay>(Error.NotFound($"Görev onayı bulunamadı (Id={id}).", "GorevOnay.NotFound"));
            return Result.Success(entity);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetByIdAsync");
            return Result.Failure<GorevOnay>(Error.Unexpected("Görev onayı getirilemedi.", ex, "GorevOnay.GetById.Failed"));
        }
    }

    public async Task<Result> AddAsync(GorevOnay entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Görev onayı boş olamaz.", "GorevOnay.Null"));
        try
        {
            entity.Olusturan = modifiedBy;
            entity.OlusturmaTarihi = DateTime.Now;
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await db.GorevOnay_Table.AddAsync(entity, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.AddAsync");
            return Result.Failure(Error.Unexpected("Görev onayı eklenemedi.", ex, "GorevOnay.Add.Failed"));
        }
    }

    public async Task<Result> UpdateAsync(GorevOnay entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Görev onayı boş olamaz.", "GorevOnay.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var existing = await db.GorevOnay_Table.FirstOrDefaultAsync(x => x.Id == entity.Id, cancellationToken);
            if (existing is null)
                return Result.Failure(Error.NotFound("Kayıt bulunamadı!", "GorevOnay.NotFound"));
            existing.PersonelId = entity.PersonelId;
            existing.GorevinSebebi = entity.GorevinSebebi;
            existing.GorevinYeri = entity.GorevinYeri;
            existing.BaslangicTarihi = entity.BaslangicTarihi;
            existing.BitisTarihi = entity.BitisTarihi;
            existing.Sure = entity.Sure;
            existing.Avans = entity.Avans;
            existing.Yevmiye = entity.Yevmiye;
            existing.ParaBirimi = entity.ParaBirimi;
            existing.AracTahsisi = entity.AracTahsisi;
            existing.AracPlakasi = entity.AracPlakasi;
            existing.PerSubeImza = entity.PerSubeImza;
            existing.PerSubeVekil = entity.PerSubeVekil;
            existing.OnayImza = entity.OnayImza;
            existing.OnayMakam = entity.OnayMakam;
            existing.OnayMakamVekil = entity.OnayMakamVekil;
            existing.GMImza = entity.GMImza;
            existing.GMVekil = entity.GMVekil;
            existing.UlasimAraci = entity.UlasimAraci;
            existing.Secildi = entity.Secildi;
            existing.GunlukYevmiye = entity.GunlukYevmiye;
            existing.Odendi = entity.Odendi;
            existing.Aciklama = entity.Aciklama;
            existing.Degistiren = entity.Degistiren;
            existing.DegistirmeTarihi = DateTime.Now;
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.UpdateAsync");
            return Result.Failure(Error.Unexpected("Görev onayı güncellenemedi.", ex, "GorevOnay.Update.Failed"));
        }
    }

    public async Task<Result> DeleteAsync(GorevOnay entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("Görev onayı boş olamaz.", "GorevOnay.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.GorevOnay_Table.Attach(entity);
            db.GorevOnay_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.DeleteAsync");
            return Result.Failure(Error.Unexpected("Görev onayı silinemedi.", ex, "GorevOnay.Delete.Failed"));
        }
    }
}
