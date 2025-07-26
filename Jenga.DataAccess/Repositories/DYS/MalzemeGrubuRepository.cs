using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.DYS;
using Jenga.Models.DYS;

namespace Jenga.DataAccess.Repositories.DYS
{
    public class MalzemeGrubuRepository : Repository<MalzemeGrubu>, IMalzemeGrubuRepository
    {
        ApplicationDbContext _db;
        public MalzemeGrubuRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

    }
}
