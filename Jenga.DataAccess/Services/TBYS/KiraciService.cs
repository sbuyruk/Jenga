using Jenga.DataAccess.Data;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public class KiraciService : IKiraciService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public KiraciService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService;
        }

        public async Task<List<Kiraci>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Kiraci_Table.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<Kiraci?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Kiraci_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<bool> AddAsync(Kiraci kiraci, CancellationToken cancellationToken = default)
        {
            if (kiraci == null) throw new ArgumentNullException(nameof(kiraci));

            var name = (kiraci.Adi ?? string.Empty).Trim();
            var surname = (kiraci.Soyadi ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(surname))
            {
                _logService?.LogWarning("KiraciService.AddAsync: Adi veya Soyadi boş olamaz.");
                return false;
            }

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.Kiraci_Table.AddAsync(kiraci, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("Kiracı eklerken hata.", ex);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Kiraci kiraci, CancellationToken cancellationToken = default)
        {
            if (kiraci == null) throw new ArgumentNullException(nameof(kiraci));

            var name = (kiraci.Adi ?? string.Empty).Trim();
            var surname = (kiraci.Soyadi ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(surname))
            {
                _logService?.LogWarning("KiraciService.UpdateAsync: Adi veya Soyadi boş olamaz.");
                return false;
            }

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.Kiraci_Table.Update(kiraci);
                await db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("Kiracı güncellerken hata.", ex);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int kiraciId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.Kiraci_Table.FirstOrDefaultAsync(x => x.Id == kiraciId, cancellationToken);
            if (entity == null) return false;

            db.Kiraci_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> AnyAsync(Expression<Func<Kiraci, bool>> predicate, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Kiraci_Table.AnyAsync(predicate, cancellationToken);
        }
    }
}
