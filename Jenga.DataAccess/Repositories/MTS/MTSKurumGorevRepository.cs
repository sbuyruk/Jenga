using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.MTS;
using Jenga.Models.MTS;

namespace Jenga.DataAccess.Repositories.MTS
{
    public class MTSKurumGorevRepository : Repository<MTSKurumGorev>, IMTSKurumGorevRepository
    {
        ApplicationDbContext _db;
        public MTSKurumGorevRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
