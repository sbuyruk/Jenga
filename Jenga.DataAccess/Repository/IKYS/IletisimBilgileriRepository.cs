using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repository.IRepository.IKYS;
using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Repository.IKYS
{
    public class IletisimBilgileriRepository : Repository<IletisimBilgileri>, IIletisimBilgileriRepository
    {
        ApplicationDbContext _db;
        public IletisimBilgileriRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

    }
}
