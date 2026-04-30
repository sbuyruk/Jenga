using Jenga.DataAccess.Data;
using Jenga.Models.Common;
using Jenga.Utility.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Common
{
    public class BolgeService : IBolgeService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;
        private List<Bolge>? _cache;

        public BolgeService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService;
        }

        public async Task<List<Bolge>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            if (_cache == null)
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                _cache = await db.Bolge_Table.AsNoTracking().ToListAsync(cancellationToken);
            }
            return _cache;
        }

        public async Task<Bolge?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Bolge_Table.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        }

        public async Task<Bolge?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                var trimmed = name.Trim();
                return await db.Bolge_Table.AsNoTracking().FirstOrDefaultAsync(b => b.Adi == trimmed, cancellationToken);
            }
            catch (Exception ex)
            {
                _logService?.LogError($"BolgeService.GetByNameAsync hata (name:{name})", ex);
                throw;
            }
        }

        public async Task<bool> AddAsync(Bolge bolge, CancellationToken cancellationToken = default)
        {
            if (bolge == null) throw new ArgumentNullException(nameof(bolge));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.Bolge_Table.AddAsync(bolge, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                _cache = null;
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("BolgeService.AddAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Bolge bolge, CancellationToken cancellationToken = default)
        {
            if (bolge == null) throw new ArgumentNullException(nameof(bolge));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.Bolge_Table.Update(bolge);
                await db.SaveChangesAsync(cancellationToken);
                _cache = null;
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("BolgeService.UpdateAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int bolgeId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.Bolge_Table.FirstOrDefaultAsync(b => b.Id == bolgeId, cancellationToken);
            if (entity == null) return false;

            db.Bolge_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            _cache = null;
            return true;
        }

        public async Task<bool> AnyAsync(Expression<Func<Bolge, bool>> predicate, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Bolge_Table.AsNoTracking().AnyAsync(predicate, cancellationToken);
        }
    }
}
