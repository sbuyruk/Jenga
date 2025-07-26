using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.MTS;
using Jenga.Models.MTS;

namespace Jenga.DataAccess.Repositories.MTS
{
    public class MTSKurumTanimRepository : Repository<MTSKurumTanim>, IMTSKurumTanimRepository
    {
        ApplicationDbContext _db;
        public MTSKurumTanimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
