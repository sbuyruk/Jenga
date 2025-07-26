using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.IKYS;
using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Repositories.IKYS
{
    public class ResmiTatilRepository : Repository<ResmiTatil>, IResmiTatilRepository
    {
        ApplicationDbContext _db;
        public ResmiTatilRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

    }
}
