using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.IKYS;
using Jenga.Models.IKYS;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Repositories.IKYS
{
    public class PersonelLocationRepository : Repository<PersonelLocation>, IPersonelLocationRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public PersonelLocationRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<PersonelLocation>> GetLocationsForPersonelAsync(int personelId, bool onlyActive = true, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            IQueryable<PersonelLocation> query = db.Set<PersonelLocation>().Include(pl => pl.Location);
            query = query.Where(pl => pl.PersonelId == personelId);
            if (onlyActive) query = query.Where(pl => pl.IsActive);
            return await query.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<List<PersonelLocation>> GetPersonelsForLocationAsync(int locationId, bool onlyActive = true, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            IQueryable<PersonelLocation> query = db.Set<PersonelLocation>().Include(pl => pl.Personel);
            query = query.Where(pl => pl.LocationId == locationId);
            if (onlyActive) query = query.Where(pl => pl.IsActive);
            return await query.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<PersonelLocation?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.Set<PersonelLocation>().Include(pl => pl.Location).Include(pl => pl.Personel)
                .AsNoTracking().FirstOrDefaultAsync(pl => pl.Id == id, cancellationToken);
        }

        public async Task<bool> AddAssignmentAsync(PersonelLocation entity, string? createdBy = null, CancellationToken cancellationToken = default)
        {
            // Reuse base AddAsync behavior for timestamps; but ensure duplicates are not created.
            await using var db = _dbFactory.CreateDbContext();

            var exists = await db.Set<PersonelLocation>()
                .AnyAsync(pl => pl.PersonelId == entity.PersonelId && pl.LocationId == entity.LocationId, cancellationToken);

            if (exists) return false;

            entity.Olusturan = string.IsNullOrWhiteSpace(createdBy) ? Environment.UserName : createdBy;
            entity.OlusturmaTarihi = DateTime.Now;

            await db.Set<PersonelLocation>().AddAsync(entity, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> RemoveAssignmentAsync(int personelId, int locationId, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            var entity = await db.Set<PersonelLocation>()
                .FirstOrDefaultAsync(pl => pl.PersonelId == personelId && pl.LocationId == locationId, cancellationToken);

            if (entity == null) return false;

            db.Set<PersonelLocation>().Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> SetPrimaryAsync(int personelId, int locationId, string? modifiedBy = null, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            // use transaction to avoid race conditions
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

            // Clear existing primary(s) for this person
            var existingPrimary = await db.Set<PersonelLocation>()
                .Where(pl => pl.PersonelId == personelId && pl.IsPrimary)
                .ToListAsync(cancellationToken);

            foreach (var e in existingPrimary)
            {
                e.IsPrimary = false;
                e.Degistiren = string.IsNullOrWhiteSpace(modifiedBy) ? Environment.UserName : modifiedBy;
                e.DegistirmeTarihi = DateTime.Now;
            }

            // Try find target
            var target = await db.Set<PersonelLocation>()
                .FirstOrDefaultAsync(pl => pl.PersonelId == personelId && pl.LocationId == locationId, cancellationToken);

            if (target != null)
            {
                target.IsPrimary = true;
                target.Degistiren = string.IsNullOrWhiteSpace(modifiedBy) ? Environment.UserName : modifiedBy;
                target.DegistirmeTarihi = DateTime.Now;
            }
            else
            {
                // create if absent
                target = new PersonelLocation
                {
                    PersonelId = personelId,
                    LocationId = locationId,
                    IsPrimary = true,
                    IsActive = true,
                    Olusturan = string.IsNullOrWhiteSpace(modifiedBy) ? Environment.UserName : modifiedBy,
                    OlusturmaTarihi = DateTime.Now
                };
                await db.Set<PersonelLocation>().AddAsync(target, cancellationToken);
            }

            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return true;
        }

        public async Task<bool> AnyAsync(Expression<Func<PersonelLocation, bool>> predicate, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.Set<PersonelLocation>().AnyAsync(predicate, cancellationToken);
        }
    }
}