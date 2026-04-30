using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public class EgitimSeviyesiService : IEgitimSeviyesiService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public EgitimSeviyesiService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
    }

    public async Task<List<EgitimSeviyesi>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.EgitimSeviyesi_Table.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<EgitimSeviyesi?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.EgitimSeviyesi_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> AddAsync(EgitimSeviyesi entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        entity.Olusturan = modifiedBy;
        entity.OlusturmaTarihi = DateTime.Now;
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await db.EgitimSeviyesi_Table.AddAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateAsync(EgitimSeviyesi entity, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var existing = await db.EgitimSeviyesi_Table.FirstOrDefaultAsync(x => x.Id == entity.Id, cancellationToken)
            ?? throw new Exception("Kayıt bulunamadı!");
        existing.Adi = entity.Adi;
        existing.KisaAdi = entity.KisaAdi;
        existing.Aciklama = entity.Aciklama;
        existing.Degistiren = entity.Degistiren;
        existing.DegistirmeTarihi = DateTime.Now;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(EgitimSeviyesi entity, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        db.EgitimSeviyesi_Table.Attach(entity);
        db.EgitimSeviyesi_Table.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
