using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repository.IRepository.DYS;
using Jenga.DataAccess.Repository.IRepository.MTS;
using Jenga.Models.IKYS;
using Jenga.Models.DYS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Repository.DYS
{
    public class EnvanterTanimRepository : Repository<EnvanterTanim>, IEnvanterTanimRepository
    {
        ApplicationDbContext _db;
        public EnvanterTanimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(EnvanterTanim obj)
        {
            _db.EnvanterTanim_Table.Update(obj);
        }
    }
}
