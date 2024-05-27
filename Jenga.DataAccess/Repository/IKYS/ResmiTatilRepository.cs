using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repository.IRepository.IKYS;
using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Repository.IKYS
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

        public void Update(ResmiTatil obj)
        {
            _db.ResmiTatil_Table.Update(obj);
        }
    }
}
