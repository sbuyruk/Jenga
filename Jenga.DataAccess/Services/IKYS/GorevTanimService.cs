using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public class GorevTanimService : IGorevTanimService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public GorevTanimService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
    }

    public async Task<List<GorevTanim>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.GorevTanim_Table.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<List<GorevTanim>> GetByBirimIdAsync(int birimId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.GorevTanim_Table.AsNoTracking().Where(x => x.BirimId == birimId).ToListAsync(cancellationToken);
    }

    public async Task<GorevTanim?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.GorevTanim_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> AddAsync(GorevTanim entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        entity.Olusturan = modifiedBy;
        entity.OlusturmaTarihi = DateTime.Now;
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await db.GorevTanim_Table.AddAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateAsync(GorevTanim entity, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var existing = await db.GorevTanim_Table.FirstOrDefaultAsync(x => x.Id == entity.Id, cancellationToken)
            ?? throw new Exception("Kayıt bulunamadı!");
        existing.BirimId = entity.BirimId;
        existing.Adi = entity.Adi;
        existing.KisaAdi = entity.KisaAdi;
        existing.PersonelId = entity.PersonelId;
        existing.Vekil = entity.Vekil;
        existing.Aktif = entity.Aktif;
        existing.Aciklama = entity.Aciklama;
        existing.Degistiren = entity.Degistiren;
        existing.DegistirmeTarihi = DateTime.Now;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(GorevTanim entity, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        db.GorevTanim_Table.Attach(entity);
        db.GorevTanim_Table.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
