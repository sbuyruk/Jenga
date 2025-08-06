using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Menu;
using Jenga.Models.Common;
using Microsoft.EntityFrameworkCore;


namespace Jenga.DataAccess.Repositories.Menu
{

    public class RolMenuRepository : Repository<RolMenu>, IRolMenuRepository
    {
        ApplicationDbContext _db;
        public RolMenuRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }


    }

}
