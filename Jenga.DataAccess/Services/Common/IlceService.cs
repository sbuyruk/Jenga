using Jenga.DataAccess.Data;
using Jenga.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Services.Common
{
    public class IlceService : IIlceService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private List<Ilce>? _cache;

        public IlceService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        public async Task<List<Ilce>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            if (_cache == null)
            {
                await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
                _cache = await db.Ilce_Table.AsNoTracking().ToListAsync(cancellationToken);
            }
            return _cache;
        }

        public async Task<Ilce?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Ilce_Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<List<Ilce>> GetByIlIdAsync(int ilId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Ilce_Table
                .AsNoTracking()
                .Where(x => x.IlId == ilId)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Ilce>> GetAktifIlcelerAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Ilce_Table
                .AsNoTracking()
                .Where(i => i.IlceAdi != null && i.IlceAdi != "Merkez" && i.Aktif == true)
                .ToListAsync(cancellationToken);
        }
    }
}
