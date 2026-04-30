using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public class IletisimBilgileriService : IIletisimBilgileriService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public IletisimBilgileriService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
    }

    public async Task<List<IletisimBilgileri>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.IletisimBilgileri_Table.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<IletisimBilgileri?> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.IletisimBilgileri_Table.AsNoTracking().FirstOrDefaultAsync(x => x.PersonelId == personelId, cancellationToken);
    }

    public async Task<IletisimBilgileri?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.IletisimBilgileri_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> AddAsync(IletisimBilgileri entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        entity.Olusturan = modifiedBy;
        entity.OlusturmaTarihi = DateTime.Now;
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await db.IletisimBilgileri_Table.AddAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateAsync(IletisimBilgileri entity, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var existing = await db.IletisimBilgileri_Table.FirstOrDefaultAsync(x => x.Id == entity.Id, cancellationToken)
            ?? throw new Exception("Kayıt bulunamadı!");
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
        return true;
    }

    public async Task<bool> DeleteAsync(IletisimBilgileri entity, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        db.IletisimBilgileri_Table.Attach(entity);
        db.IletisimBilgileri_Table.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
