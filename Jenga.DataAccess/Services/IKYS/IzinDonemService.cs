using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public class IzinDonemService : IIzinDonemService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public IzinDonemService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
    }

    public async Task<List<IzinDonem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.IzinDonem_Table.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<List<IzinDonem>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.IzinDonem_Table.AsNoTracking().Where(x => x.PersonelId == personelId).ToListAsync(cancellationToken);
    }

    public async Task<IzinDonem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.IzinDonem_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> AddAsync(IzinDonem entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        entity.Olusturan = modifiedBy;
        entity.OlusturmaTarihi = DateTime.Now;
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await db.IzinDonem_Table.AddAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateAsync(IzinDonem entity, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var existing = await db.IzinDonem_Table.FirstOrDefaultAsync(x => x.Id == entity.Id, cancellationToken)
            ?? throw new Exception("Kayıt bulunamadı!");
        existing.PersonelId = entity.PersonelId;
        existing.BaslangicTarihi = entity.BaslangicTarihi;
        existing.BitisTarihi = entity.BitisTarihi;
        existing.Adi = entity.Adi;
        existing.IzinTipi = entity.IzinTipi;
        existing.IzinHakki = entity.IzinHakki;
        existing.KullanilanIzin = entity.KullanilanIzin;
        existing.KalanIzin = entity.KalanIzin;
        existing.Birim = entity.Birim;
        existing.Aciklama = entity.Aciklama;
        existing.Degistiren = entity.Degistiren;
        existing.DegistirmeTarihi = DateTime.Now;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(IzinDonem entity, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        db.IzinDonem_Table.Attach(entity);
        db.IzinDonem_Table.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
