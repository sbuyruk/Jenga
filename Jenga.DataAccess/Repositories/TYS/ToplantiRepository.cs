using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.TYS;
using Jenga.Models.TYS;

namespace Jenga.DataAccess.Repositories.TYS
{
    public class ToplantiRepository : Repository<Toplanti>, IToplantiRepository
    {
        ApplicationDbContext _db;
        public ToplantiRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
