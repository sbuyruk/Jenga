using Jenga.DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.Models.MTS;
using Jenga.DataAccess.Repository.IRepository.MTS;

namespace Jenga.DataAccess.Repository.MTS
{
    public class FaaliyetYeriRepository : Repository<FaaliyetYeri>, IFaaliyetYeriRepository
    {
        ApplicationDbContext _db;
        public FaaliyetYeriRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(FaaliyetYeri obj)
        {
            _db.FaaliyetYeri_Table.Update(obj);
        }

    }
}
