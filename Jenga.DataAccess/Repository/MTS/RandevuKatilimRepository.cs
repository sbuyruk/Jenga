using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repository.IRepository.MTS;
using Jenga.Models.MTS;


namespace Jenga.DataAccess.Repository.MTS
{
    public class RandevuKatilimRepository : Repository<RandevuKatilim>, IRandevuKatilimRepository
    {
        ApplicationDbContext _db;
        public RandevuKatilimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(RandevuKatilim obj)
        {
            _db.RandevuKatilim_Table.Update(obj);
        }
    }
}
