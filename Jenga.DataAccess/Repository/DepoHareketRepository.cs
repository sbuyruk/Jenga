using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repository.IRepository;
using Jenga.DataAccess.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.Models.MTS;
using Jenga.Models.Ortak;
using Jenga.DataAccess.Repository.IRepository.Ortak;

namespace Jenga.DataAccess.Repository
{
    public class DepoHareketRepository : Repository<DepoHareket>, IDepoHareketRepository
    {
        ApplicationDbContext _db;
        public DepoHareketRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(DepoHareket obj)
        {
            var objFromDb = _db.DepoHareket_Table.FirstOrDefault(u => u.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.AniObjesiId = obj.AniObjesiId;
                objFromDb.DepoId = obj.DepoId;
                objFromDb.Adet = obj.Adet;
                objFromDb.GirisCikis = obj.GirisCikis;
                objFromDb.IslemYapan = obj.IslemYapan;
                objFromDb.IslemTarihi = obj.IslemTarihi;
                objFromDb.Aciklama = obj.Aciklama;
                objFromDb.Degistiren = obj.Degistiren;
                objFromDb.DegistirmeTarihi = obj.DegistirmeTarihi;

            }
        }
    
    }
}
