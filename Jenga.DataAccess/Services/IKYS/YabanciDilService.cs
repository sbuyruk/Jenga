using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public class YabanciDilService : IYabanciDilService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public YabanciDilService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
    }

    public async Task<List<YabanciDil>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.YabanciDil_Table.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<List<YabanciDil>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.YabanciDil_Table.AsNoTracking().Where(x => x.PersonelId == personelId).ToListAsync(cancellationToken);
    }

    public async Task<YabanciDil?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.YabanciDil_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> AddAsync(YabanciDil entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        entity.Olusturan = modifiedBy;
        entity.OlusturmaTarihi = DateTime.Now;
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await db.YabanciDil_Table.AddAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateAsync(YabanciDil entity, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var existing = await db.YabanciDil_Table.FirstOrDefaultAsync(x => x.Id == entity.Id, cancellationToken)
            ?? throw new Exception("Kayıt bulunamadı!");
        existing.PersonelId = entity.PersonelId;
        existing.Dil = entity.Dil;
        existing.SinavAdi = entity.SinavAdi;
        existing.SinavNotu = entity.SinavNotu;
        existing.SinavTarihi = entity.SinavTarihi;
        existing.Aciklama = entity.Aciklama;
        existing.Degistiren = entity.Degistiren;
        existing.DegistirmeTarihi = DateTime.Now;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(YabanciDil entity, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        db.YabanciDil_Table.Attach(entity);
        db.YabanciDil_Table.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
