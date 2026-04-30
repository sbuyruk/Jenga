using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public class PersonelLocationService : IPersonelLocationService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public PersonelLocationService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
    }

    public async Task<List<PersonelLocation>> GetLocationsForPersonelAsync(int personelId, bool onlyActive = true, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        IQueryable<PersonelLocation> query = db.PersonelLocation_Table.Include(pl => pl.Location).Where(pl => pl.PersonelId == personelId);
        if (onlyActive) query = query.Where(pl => pl.IsActive);
        return await query.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<List<PersonelLocation>> GetPersonelsForLocationAsync(int locationId, bool onlyActive = true, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        IQueryable<PersonelLocation> query = db.PersonelLocation_Table.Include(pl => pl.Personel).Where(pl => pl.LocationId == locationId);
        if (onlyActive) query = query.Where(pl => pl.IsActive);
        return await query.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<PersonelLocation?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.PersonelLocation_Table
            .Include(pl => pl.Location)
            .Include(pl => pl.Personel)
            .AsNoTracking()
            .FirstOrDefaultAsync(pl => pl.Id == id, cancellationToken);
    }

    public async Task<bool> AssignLocationToPersonAsync(PersonelLocation assignment, string? createdBy = null, CancellationToken cancellationToken = default)
    {
        if (assignment == null) return false;

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var exists = await db.PersonelLocation_Table
            .AnyAsync(pl => pl.PersonelId == assignment.PersonelId && pl.LocationId == assignment.LocationId, cancellationToken);
        if (exists) return false;

        assignment.Olusturan = string.IsNullOrWhiteSpace(createdBy) ? Environment.UserName : createdBy;
        assignment.OlusturmaTarihi = DateTime.Now;

        await db.PersonelLocation_Table.AddAsync(assignment, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UnassignLocationFromPersonAsync(int personelId, int locationId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var entity = await db.PersonelLocation_Table
            .FirstOrDefaultAsync(pl => pl.PersonelId == personelId && pl.LocationId == locationId, cancellationToken);
        if (entity == null) return false;

        db.PersonelLocation_Table.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SetPrimaryAssignmentAsync(int personelId, int locationId, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

        var existingPrimary = await db.PersonelLocation_Table
            .Where(pl => pl.PersonelId == personelId && pl.IsPrimary)
            .ToListAsync(cancellationToken);

        foreach (var e in existingPrimary)
        {
            e.IsPrimary = false;
            e.Degistiren = string.IsNullOrWhiteSpace(modifiedBy) ? Environment.UserName : modifiedBy;
            e.DegistirmeTarihi = DateTime.Now;
        }

        var target = await db.PersonelLocation_Table
            .FirstOrDefaultAsync(pl => pl.PersonelId == personelId && pl.LocationId == locationId, cancellationToken);

        if (target != null)
        {
            target.IsPrimary = true;
            target.Degistiren = string.IsNullOrWhiteSpace(modifiedBy) ? Environment.UserName : modifiedBy;
            target.DegistirmeTarihi = DateTime.Now;
        }
        else
        {
            target = new PersonelLocation
            {
                PersonelId = personelId,
                LocationId = locationId,
                IsPrimary = true,
                IsActive = true,
                Olusturan = string.IsNullOrWhiteSpace(modifiedBy) ? Environment.UserName : modifiedBy,
                OlusturmaTarihi = DateTime.Now
            };
            await db.PersonelLocation_Table.AddAsync(target, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return true;
    }
}
