using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public class IzinTalepService : IIzinTalepService
{
    private const string Source = nameof(IzinTalepService);
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILogService _logService;

    public IzinTalepService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
    }

    public async Task<Result<List<IzinTalep>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var list = await db.IzinTalep_Table.AsNoTracking().ToListAsync(cancellationToken);
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetAllAsync");
            return Result.Failure<List<IzinTalep>>(Error.Unexpected("İzin talepleri getirilemedi.", ex, "IzinTalep.GetAll.Failed"));
        }
    }

    public async Task<Result<List<IzinTalep>>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var list = await db.IzinTalep_Table.AsNoTracking().Where(x => x.PersonelId == personelId).ToListAsync(cancellationToken);
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetByPersonelIdAsync");
            return Result.Failure<List<IzinTalep>>(Error.Unexpected("İzin talepleri getirilemedi.", ex, "IzinTalep.GetByPersonelId.Failed"));
        }
    }

    public async Task<Result<IzinTalep>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.IzinTalep_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity is null)
                return Result.Failure<IzinTalep>(Error.NotFound($"İzin talebi bulunamadı (Id={id}).", "IzinTalep.NotFound"));
            return Result.Success(entity);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetByIdAsync");
            return Result.Failure<IzinTalep>(Error.Unexpected("İzin talebi getirilemedi.", ex, "IzinTalep.GetById.Failed"));
        }
    }

    public async Task<Result> AddAsync(IzinTalep entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("İzin talebi boş olamaz.", "IzinTalep.Null"));
        try
        {
            entity.Olusturan = modifiedBy;
            entity.OlusturmaTarihi = DateTime.Now;
            entity.Aktif ??= true;
            entity.OnayDurumu ??= 0;
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await db.IzinTalep_Table.AddAsync(entity, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.AddAsync");
            return Result.Failure(Error.Unexpected("İzin talebi eklenemedi.", ex, "IzinTalep.Add.Failed"));
        }
    }

    public async Task<Result> UpdateAsync(IzinTalep entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("İzin talebi boş olamaz.", "IzinTalep.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var existing = await db.IzinTalep_Table.FirstOrDefaultAsync(x => x.Id == entity.Id, cancellationToken);
            if (existing is null)
                return Result.Failure(Error.NotFound("Kayıt bulunamadı!", "IzinTalep.NotFound"));
            existing.PersonelId = entity.PersonelId;
            existing.IzinTipi = entity.IzinTipi;
            existing.BaslangicTarihi = entity.BaslangicTarihi;
            existing.BitisTarihi = entity.BitisTarihi;
            existing.Sure = entity.Sure;
            existing.Birim = entity.Birim;
            existing.VekilImza = entity.VekilImza;
            existing.AmirImza = entity.AmirImza;
            existing.OnayImza = entity.OnayImza;
            existing.Adres = entity.Adres;
            existing.Aktif = entity.Aktif;
            existing.IzinDonemId = entity.IzinDonemId;
            existing.OnayDurumu = entity.OnayDurumu;
            existing.EPostaGonder = entity.EPostaGonder;
            existing.Aciklama = entity.Aciklama;
            existing.Degistiren = entity.Degistiren;
            existing.DegistirmeTarihi = DateTime.Now;
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.UpdateAsync");
            return Result.Failure(Error.Unexpected("İzin talebi güncellenemedi.", ex, "IzinTalep.Update.Failed"));
        }
    }

    public async Task<Result> DeleteAsync(IzinTalep entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            return Result.Failure(Error.Validation("İzin talebi boş olamaz.", "IzinTalep.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.IzinTalep_Table.Attach(entity);
            db.IzinTalep_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.DeleteAsync");
            return Result.Failure(Error.Unexpected("İzin talebi silinemedi.", ex, "IzinTalep.Delete.Failed"));
        }
    }
}
