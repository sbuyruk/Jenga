using Jenga.DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.Models.TYS;
using Jenga.DataAccess.Repository.IRepository.TYS;

namespace Jenga.DataAccess.Repository.TYS
{
    public class ToplantiRepository : Repository<Toplanti>, IToplantiRepository
    {
        ApplicationDbContext _db;
        public ToplantiRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }       
    }
}
