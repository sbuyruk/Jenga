using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.MTS;
using Jenga.Models.MTS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.MTS
{
    public class FaaliyetKatilimRepository : Repository<FaaliyetKatilim>, IFaaliyetKatilimRepository
    {
        ApplicationDbContext _db;
        public FaaliyetKatilimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public IEnumerable<FaaliyetKatilim> IncludeIt()
        {
            IEnumerable<FaaliyetKatilim> list = _db.FaaliyetKatilim_Table.Include(m => m.Katilimci);
            return list.ToList();

        }

    }
}
