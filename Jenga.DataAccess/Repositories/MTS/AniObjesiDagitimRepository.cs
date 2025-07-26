using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.MTS;
using Jenga.Models.MTS;

namespace Jenga.DataAccess.Repositories.MTS
{
    public class AniObjesiDagitimRepository : Repository<AniObjesiDagitim>, IAniObjesiDagitimRepository
    {
        ApplicationDbContext _db;
        public AniObjesiDagitimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }


    }
}
