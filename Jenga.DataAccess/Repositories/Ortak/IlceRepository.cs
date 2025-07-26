using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Ortak;
using Jenga.Models.Ortak;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Ortak
{
    public class IlceRepository : Repository<Ilce>, IIlceRepository
    {
        ApplicationDbContext _db;
        public IlceRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }
        public async Task<List<Ilce>> GetByIlIdAsync(int ilId)
        {
            return await _db.Ilce_Table.Where(x => x.IlId == ilId).ToListAsync();
        }

    }
}
