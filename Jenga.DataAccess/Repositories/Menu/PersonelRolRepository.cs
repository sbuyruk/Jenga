using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Menu;
using Jenga.Models.Common;
using Microsoft.EntityFrameworkCore;


namespace Jenga.DataAccess.Repositories.Menu
{

    public class PersonelRolRepository : Repository<PersonelRol>, IPersonelRolRepository
    {
        ApplicationDbContext _db;
        public PersonelRolRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }


    }

}
