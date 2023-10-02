using Jenga.DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.Models.NBYS;
using Jenga.DataAccess.Repository.IRepository.NBYS;

namespace Jenga.DataAccess.Repository.NBYS
{
    public class NakitBagisciRepository : Repository<NakitBagisci>, INakitBagisciRepository
    {
        ApplicationDbContext _db;
        public NakitBagisciRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public bool Update(NakitBagisci obj)
        {
            var updated = _db.NakitBagisci_Table.Update(obj);
            return updated != null;
        }

    }
}
