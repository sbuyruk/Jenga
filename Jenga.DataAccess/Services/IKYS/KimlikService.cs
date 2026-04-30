using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public class KimlikService : IKimlikService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public KimlikService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
    }

    public async Task<List<Kimlik>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.Kimlik_Table.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<Kimlik?> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.Kimlik_Table.AsNoTracking().FirstOrDefaultAsync(x => x.PersonelId == personelId, cancellationToken);
    }

    public async Task<Kimlik?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.Kimlik_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> AddAsync(Kimlik entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        entity.Olusturan = modifiedBy;
        entity.OlusturmaTarihi = DateTime.Now;
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await db.Kimlik_Table.AddAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateAsync(Kimlik entity, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var existing = await db.Kimlik_Table.FirstOrDefaultAsync(x => x.Id == entity.Id, cancellationToken)
            ?? throw new Exception("Kayıt bulunamadı!");
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
        existing.Degistiren = entity.Degistiren;
        existing.DegistirmeTarihi = DateTime.Now;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Kimlik entity, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        db.Kimlik_Table.Attach(entity);
        db.Kimlik_Table.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
