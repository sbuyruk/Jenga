using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Ortak;
using Jenga.Models.Ortak;

namespace Jenga.DataAccess.Repositories.Ortak
{
    public class ModulTanimRepository : Repository<ModulTanim>, IModulTanimRepository
    {
        ApplicationDbContext _db;
        public ModulTanimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

    }
}
