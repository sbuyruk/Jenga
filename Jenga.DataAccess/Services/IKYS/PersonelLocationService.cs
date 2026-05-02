using Jenga.DataAccess.Data;
using Jenga.Models.IKYS;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.IKYS;

public class PersonelLocationService : IPersonelLocationService
{
    private const string Source = nameof(PersonelLocationService);
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ILogService _logService;

    public PersonelLocationService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
    }

    public async Task<Result<List<PersonelLocation>>> GetLocationsForPersonelAsync(int personelId, bool onlyActive = true, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            IQueryable<PersonelLocation> query = db.PersonelLocation_Table.Include(pl => pl.Location).Where(pl => pl.PersonelId == personelId);
            if (onlyActive) query = query.Where(pl => pl.IsActive);
            var list = await query.AsNoTracking().ToListAsync(cancellationToken);
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetLocationsForPersonelAsync");
            return Result.Failure<List<PersonelLocation>>(Error.Unexpected("Personel lokasyonları getirilemedi.", ex, "PersonelLocation.GetLocationsForPersonel.Failed"));
        }
    }

    public async Task<Result<List<PersonelLocation>>> GetPersonelsForLocationAsync(int locationId, bool onlyActive = true, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            IQueryable<PersonelLocation> query = db.PersonelLocation_Table.Include(pl => pl.Personel).Where(pl => pl.LocationId == locationId);
            if (onlyActive) query = query.Where(pl => pl.IsActive);
            var list = await query.AsNoTracking().ToListAsync(cancellationToken);
            return Result.Success(list);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetPersonelsForLocationAsync");
            return Result.Failure<List<PersonelLocation>>(Error.Unexpected("Lokasyon personelleri getirilemedi.", ex, "PersonelLocation.GetPersonelsForLocation.Failed"));
        }
    }

    public async Task<Result<PersonelLocation>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.PersonelLocation_Table
                .Include(pl => pl.Location)
                .Include(pl => pl.Personel)
                .AsNoTracking()
                .FirstOrDefaultAsync(pl => pl.Id == id, cancellationToken);
            if (entity is null)
                return Result.Failure<PersonelLocation>(Error.NotFound($"Personel lokasyonu bulunamadı (Id={id}).", "PersonelLocation.NotFound"));
            return Result.Success(entity);
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.GetByIdAsync");
            return Result.Failure<PersonelLocation>(Error.Unexpected("Personel lokasyonu getirilemedi.", ex, "PersonelLocation.GetById.Failed"));
        }
    }

    public async Task<Result> AssignLocationToPersonAsync(PersonelLocation assignment, string? createdBy = null, CancellationToken cancellationToken = default)
    {
        if (assignment is null)
            return Result.Failure(Error.Validation("Atama bilgisi boş olamaz.", "PersonelLocation.Null"));
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var exists = await db.PersonelLocation_Table
                .AnyAsync(pl => pl.PersonelId == assignment.PersonelId && pl.LocationId == assignment.LocationId, cancellationToken);
            if (exists)
                return Result.Failure(Error.Conflict("Bu personel bu lokasyona zaten atanmış.", "PersonelLocation.AlreadyAssigned"));

            db.SetCurrentUser(createdBy);
            await db.PersonelLocation_Table.AddAsync(assignment, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.AssignLocationToPersonAsync");
            return Result.Failure(Error.Unexpected("Atama yapılamadı.", ex, "PersonelLocation.Assign.Failed"));
        }
    }

    public async Task<Result> UnassignLocationFromPersonAsync(int personelId, int locationId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.PersonelLocation_Table
                .FirstOrDefaultAsync(pl => pl.PersonelId == personelId && pl.LocationId == locationId, cancellationToken);
            if (entity is null)
                return Result.Failure(Error.NotFound("Atama bulunamadı.", "PersonelLocation.NotFound"));

            db.PersonelLocation_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.UnassignLocationFromPersonAsync");
            return Result.Failure(Error.Unexpected("Atama kaldırılamadı.", ex, "PersonelLocation.Unassign.Failed"));
        }
    }

    public async Task<Result> SetPrimaryAssignmentAsync(int personelId, int locationId, string? modifiedBy = null, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

            db.SetCurrentUser(modifiedBy);
            var existingPrimary = await db.PersonelLocation_Table
                .Where(pl => pl.PersonelId == personelId && pl.IsPrimary)
                .ToListAsync(cancellationToken);

            foreach (var e in existingPrimary)
            {
                e.IsPrimary = false;
            }

            var target = await db.PersonelLocation_Table
                .FirstOrDefaultAsync(pl => pl.PersonelId == personelId && pl.LocationId == locationId, cancellationToken);

            if (target != null)
            {
                target.IsPrimary = true;
            }
            else
            {
                target = new PersonelLocation
                {
                    PersonelId = personelId,
                    LocationId = locationId,
                    IsPrimary = true,
                    IsActive = true
                };
                await db.PersonelLocation_Table.AddAsync(target, cancellationToken);
            }

            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logService.LogException(ex, $"{Source}.SetPrimaryAssignmentAsync");
            return Result.Failure(Error.Unexpected("Birincil atama ayarlanamadı.", ex, "PersonelLocation.SetPrimary.Failed"));
        }
    }
}
