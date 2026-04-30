using Jenga.DataAccess.Data;
using Jenga.Models.Common;
using Jenga.Utility.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Common
{
    public class IlService : IIlService
    {
        private static readonly string[] _excludedIlAdlari = { " ", "Boş", "Yok", "---", "Yurtdışı", "Almanya", "Diğer" };

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;
        private List<Il>? _cache;

        public IlService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService;
        }

        public async Task<List<Il>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            if (_cache == null)
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                _cache = await db.Il_Table.AsNoTracking().ToListAsync(cancellationToken);
            }
            return _cache;
        }

        public async Task<Il?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Il_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<bool> AddAsync(Il il, CancellationToken cancellationToken = default)
        {
            if (il == null) throw new ArgumentNullException(nameof(il));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.Il_Table.AddAsync(il, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                _cache = null;
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("IlService.AddAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Il il, CancellationToken cancellationToken = default)
        {
            if (il == null) throw new ArgumentNullException(nameof(il));
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.Il_Table.Update(il);
                await db.SaveChangesAsync(cancellationToken);
                _cache = null;
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("IlService.UpdateAsync hata.", ex);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int ilId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.Il_Table.FirstOrDefaultAsync(x => x.Id == ilId, cancellationToken);
            if (entity == null) return false;

            db.Il_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            _cache = null;
            return true;
        }

        public async Task<bool> AnyAsync(Expression<Func<Il, bool>> predicate, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Il_Table.AsNoTracking().AnyAsync(predicate, cancellationToken);
        }

        public async Task<List<Il>> GetAktifIllerAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Il_Table
                .AsNoTracking()
                .Where(i => i.IlAdi != null
                            && !_excludedIlAdlari.Contains(i.IlAdi)
                            && i.Aktif == true)
                .ToListAsync(cancellationToken);
        }
    }
}
