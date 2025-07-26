using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.MTS;
using Jenga.Models.MTS;

namespace Jenga.DataAccess.Repositories.MTS
{
    public class DagitimYeriTanimRepository : Repository<DagitimYeriTanim>, IDagitimYeriTanimRepository
    {
        ApplicationDbContext _db;
        public DagitimYeriTanimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

    }
}
