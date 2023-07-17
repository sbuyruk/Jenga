using Jenga.DataAccess.Data;
using Jenga.Models.MTS;
using Jenga.DataAccess.Repository.IRepository.MTS;

namespace Jenga.DataAccess.Repository.MTS
{
    public class GonderiPaketiRepository : Repository<GonderiPaketi>, IGonderiPaketiRepository
    {
        ApplicationDbContext _db;
        public GonderiPaketiRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public bool Update(GonderiPaketi obj)
        {
            var objFromDb = _db.GonderiPaketi_Table.FirstOrDefault(u => u.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.Etiket = obj.Etiket;
                objFromDb.DagitimYeriTanimId = obj.DagitimYeriTanimId;
                objFromDb.GondermeTarihi = obj.GondermeTarihi;
                objFromDb.GondermeAraci = obj.GondermeAraci;
                objFromDb.GonderiTakipNo = obj.GonderiTakipNo;
                objFromDb.IlId = obj.IlId;
                objFromDb.IlceId = obj.IlceId;
                objFromDb.Adres = obj.Adres;
                objFromDb.Aciklama = obj.Aciklama;
                objFromDb.Degistiren = obj.Degistiren;
                objFromDb.DegistirmeTarihi = obj.DegistirmeTarihi;

                var updated = _db.GonderiPaketi_Table.Update(objFromDb);
                return updated != null;
            }
            return false;
        }

    }
}
