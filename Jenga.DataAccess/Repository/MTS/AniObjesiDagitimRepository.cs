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
    public class AniObjesiDagitimRepository : Repository<AniObjesiDagitim>, IAniObjesiDagitimRepository
    {
        ApplicationDbContext _db;
        public AniObjesiDagitimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(AniObjesiDagitim obj)
        {
            var objFromDb = _db.AniObjesiDagitim_Table.FirstOrDefault(u => u.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.RandevuId = obj.RandevuId;
                objFromDb.Aciklama = obj.Aciklama;
                objFromDb.Adet= obj.Adet;
                objFromDb.AniObjesiId = obj.AniObjesiId;
                objFromDb.CikisDepoId = obj.CikisDepoId;
                objFromDb.DagitimYeriTanimId = obj.DagitimYeriTanimId;
                objFromDb.KatilimciTipi = obj.KatilimciTipi;
                objFromDb.KatilimciId = obj.KatilimciId;
                objFromDb.GetirilenAniObjesi = obj.GetirilenAniObjesi;
                objFromDb.VerilenAlinan = obj.VerilenAlinan;
            }
        }

    }
}
