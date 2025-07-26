using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.MTS;
using Jenga.Models.MTS;

namespace Jenga.DataAccess.Repositories.MTS
{
    public class AniObjesiTanimRepository : Repository<AniObjesiTanim>, IAniObjesiTanimRepository
    {
        ApplicationDbContext _db;
        public AniObjesiTanimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }


    }
}
