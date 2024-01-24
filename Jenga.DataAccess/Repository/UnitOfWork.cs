using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repository.IRepository;
using Jenga.DataAccess.Repository.IRepository.IKYS;
using Jenga.DataAccess.Repository.IRepository.MTS;
using Jenga.DataAccess.Repository.IRepository.NBYS;
using Jenga.DataAccess.Repository.IRepository.TBYS;
using Jenga.DataAccess.Repository.IRepository.Ortak;
using Jenga.DataAccess.Repository.IKYS;
using Jenga.DataAccess.Repository.MTS;
using Jenga.DataAccess.Repository.TBYS;
using Jenga.DataAccess.Repository.NBYS;
using Jenga.DataAccess.Repository.Ortak;
using Jenga.Models.MTS;

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
            GonderiPaketi = new GonderiPaketiRepository(_db);
            Kisi = new KisiRepository(_db);
            UnvanTanim = new UnvanTanimRepository(_db);
            DagitimYeriTanim = new DagitimYeriTanimRepository(_db);
            GonderiPaketi = new GonderiPaketiRepository(_db);
            AniObjesiDagitim = new AniObjesiDagitimRepository(_db);
            MTSKurumTanim = new MTSKurumTanimRepository(_db);
            MTSGorevTanim = new MTSGorevTanimRepository(_db);
            MTSUnvanTanim = new MTSUnvanTanimRepository(_db);
            MTSKurumGorev = new MTSKurumGorevRepository(_db);
            FaaliyetKatilim = new FaaliyetKatilimRepository(_db);
            Faaliyet = new FaaliyetRepository(_db);
            //Ortak
            Il = new IlRepository(_db);
            Ilce = new IlceRepository(_db);
            ModulTanim = new ModulTanimRepository(_db);
            MenuTanim = new MenuTanimRepository(_db);
            //IKYS
            Personel = new PersonelRepository(_db);
            PersonelMenu = new PersonelMenuRepository(_db);
            IsBilgileri = new IsBilgileriRepository(_db);
            GorevTanim = new GorevTanimRepository(_db);
            BirimTanim = new BirimTanimRepository(_db);
            //TBYS
            TasinmazBagisci = new TasinmazBagisciRepository(_db);
            //NBYS
            NakitBagisci = new NakitBagisciRepository(_db);
        }
        //MTS
        public IDepoTanimRepository DepoTanim { get; private set; }
        public IKaynakTanimRepository KaynakTanim { get; private set; }        
        public IAniObjesiTanimRepository AniObjesiTanim { get; private set; }        
        public IDepoHareketRepository DepoHareket { get; private set; }        
        public IDepoStokRepository DepoStok { get; private set; }        
        public IGonderiPaketiRepository GonderiPaketi{ get; private set; }        
        public IDagitimYeriTanimRepository DagitimYeriTanim { get; private set; }        
        public IKisiRepository Kisi { get; private set; }        
        public IAniObjesiDagitimRepository AniObjesiDagitim { get; private set; }        
        public IMTSKurumTanimRepository MTSKurumTanim { get; private set; }        
        public IMTSGorevTanimRepository MTSGorevTanim { get; private set; }        
        public IMTSUnvanTanimRepository MTSUnvanTanim { get; private set; }        
        public IMTSKurumGorevRepository MTSKurumGorev { get; private set; }        
        public IFaaliyetKatilimRepository FaaliyetKatilim { get; private set; }        
        public IFaaliyetRepository Faaliyet { get; private set; }        
        
        //Ortak
        public IIlRepository Il{ get; private set; }        
        public IIlceRepository Ilce { get; private set; }        
        public IModulTanimRepository ModulTanim { get; private set; }        
        public IMenuTanimRepository MenuTanim { get; private set; }

        //IKYS
        public IPersonelRepository Personel { get; private set; }        
        public IPersonelMenuRepository PersonelMenu { get; private set; }        
        public IIsBilgileriRepository IsBilgileri{ get; private set; }        
        public IGorevTanimRepository GorevTanim { get; private set; }        
        public IBirimTanimRepository BirimTanim { get; private set; }        
        public IUnvanTanimRepository UnvanTanim { get; private set; }
        //TBYS
        public ITasinmazBagisciRepository TasinmazBagisci { get; private set; }
        //NBYS
        public INakitBagisciRepository NakitBagisci { get; private set; }
        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
