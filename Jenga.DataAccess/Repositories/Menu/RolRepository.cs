using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Menu;
using Jenga.Models.Common;
using Microsoft.EntityFrameworkCore;


namespace Jenga.DataAccess.Repositories.Menu
{

    public class RolRepository : Repository<Rol>, IRolRepository
    {
        ApplicationDbContext _db;
        public RolRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
