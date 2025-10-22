using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.DYS;
using Jenga.DataAccess.Repositories.IKYS;
using Jenga.DataAccess.Repositories.Inventory;
using Jenga.DataAccess.Repositories.IRepository;
using Jenga.DataAccess.Repositories.IRepository.DYS;
using Jenga.DataAccess.Repositories.IRepository.IKYS;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.DataAccess.Repositories.IRepository.Menu;
using Jenga.DataAccess.Repositories.IRepository.MTS;
using Jenga.DataAccess.Repositories.IRepository.NBYS;
using Jenga.DataAccess.Repositories.IRepository.Ortak;
using Jenga.DataAccess.Repositories.IRepository.TBYS;
using Jenga.DataAccess.Repositories.IRepository.TYS;
using Jenga.DataAccess.Repositories.Menu;
using Jenga.DataAccess.Repositories.MTS;
using Jenga.DataAccess.Repositories.NBYS;
using Jenga.DataAccess.Repositories.Ortak;
using Jenga.DataAccess.Repositories.TBYS;
using Jenga.DataAccess.Repositories.TYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        //private readonly ApplicationDbContext _context;
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;


        public UnitOfWork(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
            //_context = _contextFactory.CreateDbContext();

            MenuItem = new MenuItemRepository(_contextFactory.CreateDbContext());
            Rol = new RolRepository(_contextFactory.CreateDbContext());
            RolMenu = new RolMenuRepository(_contextFactory.CreateDbContext());
            PersonelRol = new PersonelRolRepository(_contextFactory.CreateDbContext());
            // Inventory
            Material = new MaterialRepository(_contextFactory.CreateDbContext());
            MaterialEntry = new MaterialEntryRepository(_contextFactory.CreateDbContext());
            MaterialUnit = new MaterialUnitRepository(_contextFactory.CreateDbContext());
            MaterialCategory = new MaterialCategoryRepository(_contextFactory.CreateDbContext());
            MaterialBrand = new MaterialBrandRepository(_contextFactory.CreateDbContext());
            MaterialModel = new MaterialModelRepository(_contextFactory.CreateDbContext());
            Location = new LocationRepository(_contextFactory.CreateDbContext());
            MaterialInventory = new MaterialInventoryRepository(_contextFactory.CreateDbContext());
            MaterialMovement = new MaterialMovementRepository(_contextFactory.CreateDbContext());
            MaterialAssignment = new MaterialAssignmentRepository(_contextFactory.CreateDbContext());
            MaterialExit = new MaterialExitRepository(_contextFactory.CreateDbContext());
            MaterialTransfer = new MaterialTransferRepository(_contextFactory.CreateDbContext());
            //MTS
            DepoTanim = new DepoTanimRepository(_contextFactory.CreateDbContext());
            KaynakTanim = new KaynakTanimRepository(_contextFactory.CreateDbContext());
            AniObjesiTanim = new AniObjesiTanimRepository(_contextFactory.CreateDbContext());
            DepoHareket = new DepoHareketRepository(_contextFactory.CreateDbContext());
            DepoStok = new DepoStokRepository(_contextFactory.CreateDbContext());
            GonderiPaketi = new GonderiPaketiRepository(_contextFactory.CreateDbContext());
            Kisi = new KisiRepository(_contextFactory.CreateDbContext());
            UnvanTanim = new UnvanTanimRepository(_contextFactory.CreateDbContext());
            DagitimYeriTanim = new DagitimYeriTanimRepository(_contextFactory.CreateDbContext());
            GonderiPaketi = new GonderiPaketiRepository(_contextFactory.CreateDbContext());
            AniObjesiDagitim = new AniObjesiDagitimRepository(_contextFactory.CreateDbContext());
            MTSKurumTanim = new MTSKurumTanimRepository(_contextFactory.CreateDbContext());
            MTSGorevTanim = new MTSGorevTanimRepository(_contextFactory.CreateDbContext());
            MTSUnvanTanim = new MTSUnvanTanimRepository(_contextFactory.CreateDbContext());
            MTSKurumGorev = new MTSKurumGorevRepository(_contextFactory.CreateDbContext());
            FaaliyetKatilim = new FaaliyetKatilimRepository(_contextFactory.CreateDbContext());
            Faaliyet = new FaaliyetRepository(_contextFactory.CreateDbContext());
            FaaliyetAmaci = new FaaliyetAmaciRepository(_contextFactory.CreateDbContext());
            AramaGorusme = new AramaGorusmeRepository(_contextFactory.CreateDbContext());
            //Ortak
            Bolge = new BolgeRepository(_contextFactory.CreateDbContext());
            Il = new IlRepository(_contextFactory.CreateDbContext());
            Ilce = new IlceRepository(_contextFactory.CreateDbContext());
            ModulTanim = new ModulTanimRepository(_contextFactory.CreateDbContext());
            MenuTanim = new MenuTanimRepository(_contextFactory.CreateDbContext());
            //IKYS
            Personel = new PersonelRepository(_contextFactory.CreateDbContext());
            PersonelMenu = new PersonelMenuRepository(_contextFactory.CreateDbContext());
            IsBilgileri = new IsBilgileriRepository(_contextFactory.CreateDbContext());
            GorevTanim = new GorevTanimRepository(_contextFactory.CreateDbContext());
            BirimTanim = new BirimTanimRepository(_contextFactory.CreateDbContext());
            Kimlik = new KimlikRepository(_contextFactory.CreateDbContext());
            IletisimBilgileri = new IletisimBilgileriRepository(_contextFactory.CreateDbContext());
            ResmiTatil = new ResmiTatilRepository(_contextFactory.CreateDbContext());
            //TBYS
            TasinmazBagisci = new TasinmazBagisciRepository(_contextFactory.CreateDbContext());
            //NBYS
            NakitBagisci = new NakitBagisciRepository(_contextFactory.CreateDbContext());
            NakitBagisHareket = new NakitBagisHareketRepository(_contextFactory.CreateDbContext());
            //TYS
            Toplanti = new ToplantiRepository(_contextFactory.CreateDbContext());
            ToplantiKatilim = new ToplantiKatilimRepository(_contextFactory.CreateDbContext());
            //DYS
            EnvanterTanim = new EnvanterTanimRepository(_contextFactory.CreateDbContext());
            MalzemeGrubu = new MalzemeGrubuRepository(_contextFactory.CreateDbContext());
            MalzemeCinsi = new MalzemeCinsiRepository(_contextFactory.CreateDbContext());
            MarkaTanim = new MarkaTanimRepository(_contextFactory.CreateDbContext());
            ModelTanim = new ModelTanimRepository(_contextFactory.CreateDbContext());
            Ozellik = new OzellikRepository(_contextFactory.CreateDbContext());
            MalzemeOzellik = new MalzemeOzellikRepository(_contextFactory.CreateDbContext());
            Malzeme = new MalzemeRepository(_contextFactory.CreateDbContext());
            MalzemeYeriTanim = new MalzemeYeriTanimRepository(_contextFactory.CreateDbContext());
            MalzemeDagilim = new MalzemeDagilimRepository(_contextFactory.CreateDbContext());
            MalzemeHareket = new MalzemeHareketRepository(_contextFactory.CreateDbContext());
            Zimmet = new ZimmetRepository(_contextFactory.CreateDbContext());
        }
        
        //Common
        public IMenuItemRepository MenuItem { get; private set; }
        public IRolRepository Rol { get; private set; }
        public IRolMenuRepository RolMenu { get; private set; }
        public IPersonelRolRepository PersonelRol { get; private set; }

        //Inventory
        public IMaterialRepository Material { get; private set; }
        public IMaterialEntryRepository MaterialEntry { get; private set; }
        public IMaterialUnitRepository MaterialUnit { get; private set; }
        public IMaterialCategoryRepository MaterialCategory { get; private set; }
        public IMaterialBrandRepository MaterialBrand { get; private set; }
        public IMaterialModelRepository MaterialModel { get; private set; }
        public ILocationRepository Location { get; private set; }
        public IMaterialInventoryRepository MaterialInventory { get; private set; }
        public IMaterialMovementRepository MaterialMovement { get; private set; }
        public IMaterialAssignmentRepository MaterialAssignment { get; private set; }
        public IMaterialExitRepository MaterialExit { get; private set; }
        public IMaterialTransferRepository MaterialTransfer { get; private set; }
        //MTS
        public IDepoTanimRepository DepoTanim { get; private set; }
        public IKaynakTanimRepository KaynakTanim { get; private set; }
        public IAniObjesiTanimRepository AniObjesiTanim { get; private set; }
        public IDepoHareketRepository DepoHareket { get; private set; }
        public IDepoStokRepository DepoStok { get; private set; }
        public IGonderiPaketiRepository GonderiPaketi { get; private set; }
        public IDagitimYeriTanimRepository DagitimYeriTanim { get; private set; }
        public IKisiRepository Kisi { get; private set; }
        public IAniObjesiDagitimRepository AniObjesiDagitim { get; private set; }
        public IMTSKurumTanimRepository MTSKurumTanim { get; private set; }
        public IMTSGorevTanimRepository MTSGorevTanim { get; private set; }
        public IMTSUnvanTanimRepository MTSUnvanTanim { get; private set; }
        public IMTSKurumGorevRepository MTSKurumGorev { get; private set; }
        public IFaaliyetKatilimRepository FaaliyetKatilim { get; private set; }
        public IFaaliyetRepository Faaliyet { get; private set; }
        public IFaaliyetAmaciRepository FaaliyetAmaci { get; private set; }
        public IAramaGorusmeRepository AramaGorusme { get; private set; }

        //Ortak
        public IBolgeRepository Bolge { get; private set; }
        public IIlRepository Il { get; private set; }
        public IIlceRepository Ilce { get; private set; }
        public IModulTanimRepository ModulTanim { get; private set; }
        public IMenuTanimRepository MenuTanim { get; private set; }

        //IKYS
        public IPersonelRepository Personel { get; private set; }
        public IPersonelMenuRepository PersonelMenu { get; private set; }
        public IIsBilgileriRepository IsBilgileri { get; private set; }
        public IGorevTanimRepository GorevTanim { get; private set; }
        public IBirimTanimRepository BirimTanim { get; private set; }
        public IUnvanTanimRepository UnvanTanim { get; private set; }
        public IKimlikRepository Kimlik { get; private set; }
        public IIletisimBilgileriRepository IletisimBilgileri { get; private set; }
        public IResmiTatilRepository ResmiTatil { get; private set; }
        //TBYS
        public ITasinmazBagisciRepository TasinmazBagisci { get; private set; }
        //NBYS
        public INakitBagisciRepository NakitBagisci { get; private set; }
        public INakitBagisHareketRepository NakitBagisHareket { get; private set; }
        //NBYS
        public IToplantiRepository Toplanti { get; private set; }
        public IToplantiKatilimRepository ToplantiKatilim { get; private set; }
        //DYS
        public IEnvanterTanimRepository EnvanterTanim { get; private set; }
        public IMalzemeGrubuRepository MalzemeGrubu { get; private set; }
        public IMalzemeCinsiRepository MalzemeCinsi { get; private set; }
        public IMarkaTanimRepository MarkaTanim { get; private set; }
        public IModelTanimRepository ModelTanim { get; private set; }
        public IOzellikRepository Ozellik { get; private set; }
        public IMalzemeOzellikRepository MalzemeOzellik { get; private set; }
        public IMalzemeRepository Malzeme { get; private set; }
        public IMalzemeYeriTanimRepository MalzemeYeriTanim { get; private set; }
        public IMalzemeDagilimRepository MalzemeDagilim { get; private set; }
        public IMalzemeHareketRepository MalzemeHareket { get; private set; }
        public IZimmetRepository Zimmet { get; private set; }
        
        //public void Save()
        //{
        //    _context.SaveChanges();
        //}

        //public async Task<int> CommitAsync()
        //{
        //    return await _context.SaveChangesAsync();
        //}

        //public async Task<bool> SaveAsync(CancellationToken cancellationToken = default)
        //{
        //    var changes = await _context.SaveChangesAsync(cancellationToken);
        //    return changes > 0;
        //}

        //public void Dispose()
        //{
        //    _context.Dispose();
        //}

        //public ValueTask DisposeAsync()
        //{
        //    return _context?.DisposeAsync() ?? ValueTask.CompletedTask;
        //}
    }
}
