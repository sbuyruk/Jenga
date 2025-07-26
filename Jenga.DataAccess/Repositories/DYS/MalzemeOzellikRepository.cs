using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.DYS;
using Jenga.Models.DYS;

namespace Jenga.DataAccess.Repositories.DYS
{
    public class MalzemeOzellikRepository : Repository<MalzemeOzellik>, IMalzemeOzellikRepository
    {
        ApplicationDbContext _db;
        public MalzemeOzellikRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

    }
}
