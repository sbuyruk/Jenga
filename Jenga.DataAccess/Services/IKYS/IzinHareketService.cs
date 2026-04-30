using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public class IzinHareketService : IIzinHareketService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public IzinHareketService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
    }

    public async Task<List<IzinHareket>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.IzinHareket_Table.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<List<IzinHareket>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.IzinHareket_Table.AsNoTracking().Where(x => x.PersonelId == personelId).ToListAsync(cancellationToken);
    }

    public async Task<IzinHareket?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.IzinHareket_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> AddAsync(IzinHareket entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        entity.Olusturan = modifiedBy;
        entity.OlusturmaTarihi = DateTime.Now;
        entity.Mahsup ??= false;
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await db.IzinHareket_Table.AddAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateAsync(IzinHareket entity, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var existing = await db.IzinHareket_Table.FirstOrDefaultAsync(x => x.Id == entity.Id, cancellationToken)
            ?? throw new Exception("Kayıt bulunamadı!");
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
        existing.Degistiren = entity.Degistiren;
        existing.DegistirmeTarihi = DateTime.Now;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(IzinHareket entity, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        db.IzinHareket_Table.Attach(entity);
        db.IzinHareket_Table.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
