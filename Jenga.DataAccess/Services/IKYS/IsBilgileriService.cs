using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public class IsBilgileriService : IIsBilgileriService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public IsBilgileriService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
    }

    public async Task<List<IsBilgileri>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.IsBilgileri_Table.AsNoTracking().Include(ib => ib.UnvanTanim).ToListAsync(cancellationToken);
    }

    public async Task<IsBilgileri?> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.IsBilgileri_Table.AsNoTracking().FirstOrDefaultAsync(x => x.PersonelId == personelId, cancellationToken);
    }

    public async Task<IsBilgileri?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.IsBilgileri_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> AddAsync(IsBilgileri entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        entity.Olusturan = modifiedBy;
        entity.OlusturmaTarihi = DateTime.Now;
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await db.IsBilgileri_Table.AddAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateAsync(IsBilgileri entity, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var existing = await db.IsBilgileri_Table.FirstOrDefaultAsync(x => x.Id == entity.Id, cancellationToken)
            ?? throw new Exception("Kayıt bulunamadı!");
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
        existing.Degistiren = entity.Degistiren;
        existing.DegistirmeTarihi = DateTime.Now;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(IsBilgileri entity, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        db.IsBilgileri_Table.Attach(entity);
        db.IsBilgileri_Table.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
