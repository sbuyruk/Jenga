using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.IKYS;

public class PersonelService : IPersonelService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public PersonelService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
    }

    public async Task<List<Personel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.Personel_Table.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<Personel?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.Personel_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> AddAsync(Personel personel, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await db.Personel_Table.AddAsync(personel, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateAsync(Personel personel, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var existing = await db.Personel_Table.FirstOrDefaultAsync(x => x.Id == personel.Id, cancellationToken)
            ?? throw new Exception("Kayıt bulunamadı!");

        existing.Adi = personel.Adi;
        existing.Soyadi = personel.Soyadi;
        existing.KullaniciAdi = personel.KullaniciAdi;
        existing.Asker_sivil = personel.Asker_sivil;
        existing.Aciklama = personel.Aciklama;
        existing.SicilNo = personel.SicilNo;
        existing.Tahsili = personel.Tahsili;

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Personel personel, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        db.Personel_Table.Attach(personel);
        db.Personel_Table.Remove(personel);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> AnyAsync(Expression<Func<Personel, bool>> predicate)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Personel_Table.AnyAsync(predicate);
    }

    public Task<bool> UpdatePersonelAndSaveAsync(Personel personel, string? currentUserName, CancellationToken cancellationToken = default)
        => UpdateAsync(personel, cancellationToken);

    public Task<bool> DeletePersonelAndSaveAsync(Personel personel, string? currentUserName, CancellationToken cancellationToken = default)
        => DeleteAsync(personel, cancellationToken);

    public async Task<List<Personel>> GetKadroluPersonelAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.Personel_Table.AsNoTracking()
            .Include(p => p.IsBilgileri)
            .Where(p => p.IsBilgileri != null && p.IsBilgileri.CalismaDurumu != null
                        && p.IsBilgileri.CalismaDurumu == "1" && p.Tipi == 1)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Personel>> GetCalisanPersonelAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.Personel_Table.AsNoTracking()
            .Include(p => p.IsBilgileri)
            .Where(p => p.IsBilgileri != null && p.IsBilgileri.CalismaDurumu != null
                        && p.IsBilgileri.CalismaDurumu == "1")
            .ToListAsync(cancellationToken);
    }
}
