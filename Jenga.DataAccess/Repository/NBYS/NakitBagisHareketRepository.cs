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
    public class NakitBagisHareketRepository : Repository<NakitBagisHareket>, INakitBagisHareketRepository
    {
        ApplicationDbContext _db;
        public NakitBagisHareketRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public bool Update(NakitBagisHareket obj)
        {
            var updated = _db.NakitBagisHareket_Table.Update(obj);
            return updated != null;
        }

    }
}
