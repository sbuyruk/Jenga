using Jenga.DataAccess.Data;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public class OdemePlaniService : IOdemePlaniService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public OdemePlaniService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService;
        }

        public async Task<List<OdemePlani>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.OdemePlani_Table.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<OdemePlani?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.OdemePlani_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<bool> AddAsync(OdemePlani odemePlani, CancellationToken cancellationToken = default)
        {
            if (odemePlani == null) throw new ArgumentNullException(nameof(odemePlani));

            if (!odemePlani.SozlesmeId.HasValue)
            {
                _logService?.LogWarning("OdemePlaniService.AddAsync: SozlesmeId gerekli.");
                return false;
            }

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.OdemePlani_Table.AddAsync(odemePlani, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("Ödeme planı eklerken hata.", ex);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(OdemePlani odemePlani, CancellationToken cancellationToken = default)
        {
            if (odemePlani == null) throw new ArgumentNullException(nameof(odemePlani));

            if (!odemePlani.SozlesmeId.HasValue)
            {
                _logService?.LogWarning("OdemePlaniService.UpdateAsync: SozlesmeId gerekli.");
                return false;
            }

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.OdemePlani_Table.Update(odemePlani);
                await db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("Ödeme planı güncellerken hata.", ex);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.OdemePlani_Table.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity == null) return false;

            db.OdemePlani_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> AnyAsync(Expression<Func<OdemePlani, bool>> predicate, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.OdemePlani_Table.AnyAsync(predicate, cancellationToken);
        }
    }
}
