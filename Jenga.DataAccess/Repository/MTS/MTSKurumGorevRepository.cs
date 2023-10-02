using Jenga.DataAccess.Data;
using Jenga.Models.MTS;
using Jenga.DataAccess.Repository.IRepository.MTS;

namespace Jenga.DataAccess.Repository.MTS
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

        public void Update(MTSKurumGorev obj)
        {
            _db.MTSKurumGorev_Table.Update(obj);
        }

    }
}
