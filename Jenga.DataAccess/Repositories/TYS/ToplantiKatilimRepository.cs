using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.TYS;
using Jenga.Models.MTS;

namespace Jenga.DataAccess.Repositories.TYS
{
    public class ToplantiKatilimRepository : Repository<ToplantiKatilim>, IToplantiKatilimRepository
    {
        ApplicationDbContext _db;
        public ToplantiKatilimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

    }
}
