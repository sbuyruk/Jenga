using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.DYS;
using Jenga.Models.DYS;

namespace Jenga.DataAccess.Repositories.DYS
{
    public class MarkaTanimRepository : Repository<MarkaTanim>, IMarkaTanimRepository
    {
        ApplicationDbContext _db;
        public MarkaTanimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

    }
}
