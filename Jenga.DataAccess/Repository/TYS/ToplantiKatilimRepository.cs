using Jenga.DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.Models.TYS;
using Jenga.DataAccess.Repository.IRepository.TYS;
using Jenga.Models.MTS;

namespace Jenga.DataAccess.Repository.TYS
{
    public class ToplantiKatilimRepository : Repository<ToplantiKatilim>, IToplantiKatilimRepository
    {
        ApplicationDbContext _db;
        public ToplantiKatilimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(ToplantiKatilim obj)
        {
            _db.ToplantiKatilim_Table.Update(obj);
        }

    }
}
