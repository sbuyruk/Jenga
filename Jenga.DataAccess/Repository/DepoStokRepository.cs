using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repository.IRepository;
using Jenga.DataAccess.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.Models.MTS;

namespace Jenga.DataAccess.Repository
{
    public class DepoStokRepository : Repository<DepoStok>, IDepoStokRepository
    {
        ApplicationDbContext _db;
        public DepoStokRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(DepoStok obj)
        {
            var objFromDb = _db.DepoStok_Table.FirstOrDefault(u => u.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.AniObjesiId = obj.AniObjesiId;
                objFromDb.DepoId = obj.DepoId;
                objFromDb.SonAdet = obj.SonAdet;
                objFromDb.SonIslemYapan = obj.SonIslemYapan;
                objFromDb.SonIslemTarihi = obj.SonIslemTarihi;
                objFromDb.Aciklama = obj.Aciklama;
                objFromDb.Degistiren = obj.Degistiren;
                objFromDb.DegistirmeTarihi = obj.DegistirmeTarihi;

            }
        }
    
    }
}
