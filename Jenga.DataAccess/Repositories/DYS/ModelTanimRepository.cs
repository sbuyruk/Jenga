using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.DYS;
using Jenga.Models.DYS;

namespace Jenga.DataAccess.Repositories.DYS
{
    public class ModelTanimRepository : Repository<ModelTanim>, IModelTanimRepository
    {
        ApplicationDbContext _db;
        public ModelTanimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

    }
}
