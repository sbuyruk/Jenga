using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Ortak;
using Jenga.Models.Ortak;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Ortak
{
    public class IlRepository : Repository<Il>, IIlRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        public IlRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }

        // Async Save örneği
        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<SelectListItem>> GetIlDDL(int bolgeId = 0, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();

            var query = db.Il_Table
                .Select(m => new
                {
                    m.Id,
                    Adi = m.IlAdi,
                    Bolge = m.BolgeId
                })
                .AsQueryable();

            if (bolgeId > 0)
            {
                query = query.Where(i => i.Bolge == bolgeId);
            }

            var ilList = await query.ToListAsync(cancellationToken);

            var ilDropdownList = ilList
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Adi,
                }).ToList();

            return ilDropdownList;
        }
    }
}