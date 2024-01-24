using Jenga.DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.Models.MTS;
using Jenga.DataAccess.Repository.IRepository.MTS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repository.MTS
{
    public class FaaliyetRepository : Repository<Faaliyet>, IFaaliyetRepository
    {
        ApplicationDbContext _db;
        public FaaliyetRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(Faaliyet obj)
        {
            _db.Faaliyet_Table.Update(obj);
        }
        public IEnumerable<Faaliyet> IncludeIt(DateTime? baslangicTarihi)
        {
            IEnumerable<Faaliyet> list = _db.Faaliyet_Table.Where(o => o.BaslangicTarihi> baslangicTarihi).Include(m => m.FaaliyetKatilims);
               
            return list.ToList();

        }
    }
}
