using Jenga.DataAccess.Data;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public class OdemeService : IOdemeService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public OdemeService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService;
        }

        public async Task<List<Odeme>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Odeme_Table.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<List<Odeme>> GetAllAsyncKiralar(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Odeme_Table
                .Where(o => o.OdemePlaniId != null)
                .Join(db.OdemePlani_Table,
                      o => o.OdemePlaniId,
                      p => p.Id,
                      (o, p) => o)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<Odeme?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Odeme_Table.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        }

        public async Task<Odeme?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Odeme_Table.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        }

        public async Task<List<Odeme>> GetBySozlesmeIdAsync(int sozlesmeId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Odeme_Table.AsNoTracking().Where(o => o.SozlesmeId == sozlesmeId).ToListAsync(cancellationToken);
        }

        public async Task<List<Odeme>> GetByKiraciIdAsync(int kiraciId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Odeme_Table.AsNoTracking().Where(o => o.KiraciId == kiraciId).ToListAsync(cancellationToken);
        }

        public async Task<List<Odeme>> GetByOdemePlaniIdAsync(int odemePlaniId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Odeme_Table.AsNoTracking().Where(o => o.OdemePlaniId == odemePlaniId).ToListAsync(cancellationToken);
        }

        public async Task<bool> AddAsync(Odeme odeme, CancellationToken cancellationToken = default)
        {
            if (odeme == null) throw new ArgumentNullException(nameof(odeme));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.Odeme_Table.AddAsync(odeme, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("OdemeService.AddAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Odeme odeme, CancellationToken cancellationToken = default)
        {
            if (odeme == null) throw new ArgumentNullException(nameof(odeme));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.Odeme_Table.Update(odeme);
                await db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("OdemeService.UpdateAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int odemeId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.Odeme_Table.FirstOrDefaultAsync(o => o.Id == odemeId, cancellationToken);
            if (entity == null) return false;

            db.Odeme_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> AnyAsync(Expression<Func<Odeme, bool>> predicate, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Odeme_Table.AnyAsync(predicate, cancellationToken);
        }

        public async Task<(bool CanDelete, string? Reason)> CanDeleteAsync(int id)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var exists = await db.Odeme_Table.AsNoTracking().AnyAsync(o => o.Id == id);
            if (!exists) return (false, "Kayıt bulunamadı.");
            return (true, null);
        }
    }
}
