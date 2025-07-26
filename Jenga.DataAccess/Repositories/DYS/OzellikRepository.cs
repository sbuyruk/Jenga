using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.DYS;
using Jenga.Models.DYS;

namespace Jenga.DataAccess.Repositories.DYS
{
    public class OzellikRepository : Repository<Ozellik>, IOzellikRepository
    {
        ApplicationDbContext _db;
        public OzellikRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

    }
}
