using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.TBYS;
using Jenga.Models.TBYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.TBYS
{
    public class TasinmazRepository : Repository<Tasinmaz>, ITasinmazRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        public TasinmazRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }

        // Eğer navigation property ile ilişkili veri çekmek istersen:
        public async Task<Tasinmaz?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.Tasinmaz_Table
                //.Include(m => m.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        }
    }
}