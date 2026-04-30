using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public class IzinTalepService : IIzinTalepService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public IzinTalepService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
    }

    public async Task<List<IzinTalep>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.IzinTalep_Table.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<List<IzinTalep>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.IzinTalep_Table.AsNoTracking().Where(x => x.PersonelId == personelId).ToListAsync(cancellationToken);
    }

    public async Task<IzinTalep?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.IzinTalep_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> AddAsync(IzinTalep entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        entity.Olusturan = modifiedBy;
        entity.OlusturmaTarihi = DateTime.Now;
        entity.Aktif ??= true;
        entity.OnayDurumu ??= 0;
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await db.IzinTalep_Table.AddAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateAsync(IzinTalep entity, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var existing = await db.IzinTalep_Table.FirstOrDefaultAsync(x => x.Id == entity.Id, cancellationToken)
            ?? throw new Exception("Kayıt bulunamadı!");
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
        return true;
    }

    public async Task<bool> DeleteAsync(IzinTalep entity, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        db.IzinTalep_Table.Attach(entity);
        db.IzinTalep_Table.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
