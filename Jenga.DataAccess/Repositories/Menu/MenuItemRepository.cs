using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Menu;
using Jenga.Models.Common;
using Microsoft.EntityFrameworkCore;


namespace Jenga.DataAccess.Repositories.Menu
{

    public class MenuItemRepository : Repository<MenuItem>, IMenuItemRepository
    {
        ApplicationDbContext _db;
        public MenuItemRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public async Task<List<MenuItem>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var result = await _db.MenuItem_Table.ToListAsync(cancellationToken);
            return result;
        }
        public async Task<MenuItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _db.Set<MenuItem>()
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        }
        public async Task AddAsync(MenuItem item, CancellationToken cancellationToken = default)
        {
            await _db.Set<MenuItem>().AddAsync(item, cancellationToken);
        }

    }

}
