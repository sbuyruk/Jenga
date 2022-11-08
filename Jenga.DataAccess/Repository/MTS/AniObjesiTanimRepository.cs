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
    public class AniObjesiTanimRepository : Repository<AniObjesiTanim>, IAniObjesiTanimRepository
    {
        ApplicationDbContext _db;
        public AniObjesiTanimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(AniObjesiTanim obj)
        {
            var objFromDb = _db.AniObjesiTanim_Table.FirstOrDefault(u => u.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.Adi = obj.Adi;
                objFromDb.Aciklama = obj.Aciklama;
                objFromDb.KaynakId = obj.KaynakId;
                objFromDb.StokluMu = obj.StokluMu;
            }
        }

    }
}
