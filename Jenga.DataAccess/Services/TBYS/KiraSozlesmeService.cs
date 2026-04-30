using Jenga.DataAccess.Data;
using Jenga.Models.TBYS;
using Jenga.Utility.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS
{
    public class KiraSozlesmeService : IKiraSozlesmeService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogService _logService;

        public KiraSozlesmeService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogService logService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _logService = logService;
        }

        public async Task<List<KiraSozlesme>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.KiraSozlesme_Table.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<KiraSozlesme?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.KiraSozlesme_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<bool> AddAsync(KiraSozlesme sozlesme, CancellationToken cancellationToken = default)
        {
            if (sozlesme == null) throw new ArgumentNullException(nameof(sozlesme));

            var hasParty = sozlesme.KiraciId.HasValue || sozlesme.SozBasTar.HasValue || !string.IsNullOrWhiteSpace(sozlesme.SozlesmeDurumu);
            if (!hasParty)
            {
                _logService?.LogWarning("KiraSozlesmeService.AddAsync: KiraciId, TasinmazId veya SozlesmeNo gerekli.");
                return false;
            }

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                await db.KiraSozlesme_Table.AddAsync(sozlesme, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("Kira sözleşmesi eklerken hata.", ex);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(KiraSozlesme sozlesme, CancellationToken cancellationToken = default)
        {
            if (sozlesme == null) throw new ArgumentNullException(nameof(sozlesme));

            var hasParty = sozlesme.KiraciId.HasValue || sozlesme.SozBasTar.HasValue || !string.IsNullOrWhiteSpace(sozlesme.SozlesmeDurumu);
            if (!hasParty)
            {
                _logService?.LogWarning("KiraSozlesmeService.UpdateAsync: KiraciId, TasinmazId veya SozlesmeNo gerekli.");
                return false;
            }

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                db.KiraSozlesme_Table.Update(sozlesme);
                await db.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logService?.LogError("Kira sözleşmesi güncellerken hata.", ex);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int sozlesmeId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var entity = await db.KiraSozlesme_Table.FirstOrDefaultAsync(x => x.Id == sozlesmeId, cancellationToken);
            if (entity == null) return false;

            db.KiraSozlesme_Table.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> AnyAsync(Expression<Func<KiraSozlesme, bool>> predicate, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.KiraSozlesme_Table.AnyAsync(predicate, cancellationToken);
        }
    }
}
