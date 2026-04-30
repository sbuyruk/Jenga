using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public class GorevOnayService : IGorevOnayService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public GorevOnayService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
    }

    public async Task<List<GorevOnay>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.GorevOnay_Table.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<List<GorevOnay>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.GorevOnay_Table.AsNoTracking().Where(x => x.PersonelId == personelId).ToListAsync(cancellationToken);
    }

    public async Task<GorevOnay?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.GorevOnay_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> AddAsync(GorevOnay entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        entity.Olusturan = modifiedBy;
        entity.OlusturmaTarihi = DateTime.Now;
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await db.GorevOnay_Table.AddAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateAsync(GorevOnay entity, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var existing = await db.GorevOnay_Table.FirstOrDefaultAsync(x => x.Id == entity.Id, cancellationToken)
            ?? throw new Exception("Kayıt bulunamadı!");
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
        return true;
    }

    public async Task<bool> DeleteAsync(GorevOnay entity, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        db.GorevOnay_Table.Attach(entity);
        db.GorevOnay_Table.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
