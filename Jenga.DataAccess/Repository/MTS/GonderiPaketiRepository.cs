using Jenga.DataAccess.Data;
using Jenga.Models.MTS;
using Jenga.DataAccess.Repository.IRepository.MTS;

namespace Jenga.DataAccess.Repository.MTS
{
    public class GonderiPaketiRepository : Repository<GonderiPaketi>, IGonderiPaketiRepository
    {
        ApplicationDbContext _db;
        public GonderiPaketiRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        
    }
}
