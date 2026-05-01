using Jenga.DataAccess.Data;
using Jenga.Models.Common;
using Jenga.Utility.Logging;
using Jenga.Utility.Results;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.Common
{
    public class PersonelRoleService : IPersonelRoleService
    {
        private const string Source = nameof(PersonelRoleService);
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public PersonelRoleService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        public async Task<Result<List<PersonelRole>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.PersonelRol_Table.AsNoTracking().ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetAllAsync");
                return Result.Failure<List<PersonelRole>>(Error.Unexpected("Personel rolleri getirilemedi.", ex, "PersonelRole.GetAll.Failed"));
            }
        }

        public async Task<Result<PersonelRole>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var entity = await db.PersonelRol_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (entity is null)
                    return Result.Failure<PersonelRole>(Error.NotFound($"Personel rolü bulunamadı (Id={id}).", "PersonelRole.NotFound"));
                return Result.Success(entity);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetByIdAsync");
                return Result.Failure<PersonelRole>(Error.Unexpected("Personel rolü getirilemedi.", ex, "PersonelRole.GetById.Failed"));
            }
        }

        public async Task<Result<List<PersonelRole>>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var list = await db.PersonelRol_Table
                    .AsNoTracking()
                    .Where(pr => pr.PersonelId == personelId)
                    .ToListAsync(cancellationToken);
                return Result.Success(list);
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.GetByPersonelIdAsync");
                return Result.Failure<List<PersonelRole>>(Error.Unexpected("Personel rolleri getirilemedi.", ex, "PersonelRole.GetByPersonelId.Failed"));
            }
        }

        public async Task<Result> AddAsync(PersonelRole personelRole, CancellationToken cancellationToken = default)
        {
            if (personelRole == null)
                return Result.Failure(Error.Validation("Personel rolü boş olamaz.", "PersonelRole.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.PersonelRol_Table.AddAsync(personelRole, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.AddAsync");
                return Result.Failure(Error.Unexpected("Personel rolü eklenemedi.", ex, "PersonelRole.Add.Failed"));
            }
        }

        public async Task<Result> UpdateAsync(PersonelRole personelRole, CancellationToken cancellationToken = default)
        {
            if (personelRole == null)
                return Result.Failure(Error.Validation("Personel rolü boş olamaz.", "PersonelRole.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.PersonelRol_Table.Update(personelRole);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.UpdateAsync");
                return Result.Failure(Error.Unexpected("Personel rolü güncellenemedi.", ex, "PersonelRole.Update.Failed"));
            }
        }

        public async Task<Result> DeleteAsync(PersonelRole personelRole, CancellationToken cancellationToken = default)
        {
            if (personelRole == null)
                return Result.Failure(Error.Validation("Personel rolü boş olamaz.", "PersonelRole.Null"));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.PersonelRol_Table.Remove(personelRole);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"{Source}.DeleteAsync");
                return Result.Failure(Error.Unexpected("Personel rolü silinemedi.", ex, "PersonelRole.Delete.Failed"));
            }
        }
    }
}
