using Jenga.DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.Models.MTS;
using Jenga.DataAccess.Repository.IRepository.MTS;
using Microsoft.EntityFrameworkCore;
using Jenga.Utility;

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

        public IEnumerable<Faaliyet> IncludeIt(DateTime? baslangicTarihi)
        {
            IEnumerable<Faaliyet> list;
            if (baslangicTarihi == null || baslangicTarihi < ProjectConstants.ILK_TARIH)
            {
                list = _db.Faaliyet_Table.Include(m => m.FaaliyetKatilims);
            }
            else
            {
                list = _db.Faaliyet_Table.Where(o => o.BaslangicTarihi> baslangicTarihi).Include(m => m.FaaliyetKatilims);
            }
               
            return list.ToList();

        }
    }
}
