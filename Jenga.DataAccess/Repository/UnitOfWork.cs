using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repository.IRepository;
using Jenga.DataAccess.Repository.IRepository.IKYS;
using Jenga.DataAccess.Repository.IRepository.MTS;
using Jenga.DataAccess.Repository.IRepository.Ortak;
using Jenga.DataAccess.Repository.IKYS;
using Jenga.DataAccess.Repository.MTS;
using Jenga.DataAccess.Repository.Ortak;

namespace Jenga.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;
        public UnitOfWork(ApplicationDbContext db) 
        {
            _db = db;
            //MTS
            DepoTanim= new DepoTanimRepository(_db);
            KaynakTanim= new KaynakTanimRepository(_db);
            AniObjesiTanim= new AniObjesiTanimRepository(_db);
            DepoHareket= new DepoHareketRepository(_db);
            DepoStok= new DepoStokRepository(_db);
            //Ortak
            ModulTanim = new ModulTanimRepository(_db);
            MenuTanim = new MenuTanimRepository(_db);
            Personel = new PersonelRepository(_db);
            PersonelMenu = new PersonelMenuRepository(_db);
        }
        public IDepoTanimRepository DepoTanim { get; private set; }
        public IKaynakTanimRepository KaynakTanim { get; private set; }        
        public IAniObjesiTanimRepository AniObjesiTanim { get; private set; }        
        public IDepoHareketRepository DepoHareket { get; private set; }        
        public IDepoStokRepository DepoStok { get; private set; }        
        public IModulTanimRepository ModulTanim { get; private set; }        
        public IMenuTanimRepository MenuTanim { get; private set; }        
        public IPersonelRepository Personel { get; private set; }        
        public IPersonelMenuRepository PersonelMenu { get; private set; }        

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
