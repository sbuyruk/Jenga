using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.DYS;
using Jenga.DataAccess.Repositories.IKYS;
using Jenga.DataAccess.Repositories.IRepository;
using Jenga.DataAccess.Repositories.IRepository.DYS;
using Jenga.DataAccess.Repositories.IRepository.IKYS;
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
    public class UnitOfWork : IUnitOfWork, IAsyncDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;


        public UnitOfWork(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
            _context = _contextFactory.CreateDbContext();

            MenuItem = new MenuItemRepository(_context);
            Rol = new RolRepository(_context);
            RolMenu = new RolMenuRepository(_context);
            PersonelRol = new PersonelRolRepository(_context);

            //MTS
            DepoTanim = new DepoTanimRepository(_context);
            KaynakTanim = new KaynakTanimRepository(_context);
            AniObjesiTanim = new AniObjesiTanimRepository(_context);
            DepoHareket = new DepoHareketRepository(_context);
            DepoStok = new DepoStokRepository(_context);
            GonderiPaketi = new GonderiPaketiRepository(_context);
            Kisi = new KisiRepository(_context);
            UnvanTanim = new UnvanTanimRepository(_context);
            DagitimYeriTanim = new DagitimYeriTanimRepository(_context);
            GonderiPaketi = new GonderiPaketiRepository(_context);
            AniObjesiDagitim = new AniObjesiDagitimRepository(_context);
            MTSKurumTanim = new MTSKurumTanimRepository(_context);
            MTSGorevTanim = new MTSGorevTanimRepository(_context);
            MTSUnvanTanim = new MTSUnvanTanimRepository(_context);
            MTSKurumGorev = new MTSKurumGorevRepository(_context);
            FaaliyetKatilim = new FaaliyetKatilimRepository(_context);
            Faaliyet = new FaaliyetRepository(_context);
            FaaliyetAmaci = new FaaliyetAmaciRepository(_context);
            AramaGorusme = new AramaGorusmeRepository(_context);
            //Ortak
            Bolge = new BolgeRepository(_context);
            Il = new IlRepository(_context);
            Ilce = new IlceRepository(_context);
            ModulTanim = new ModulTanimRepository(_context);
            MenuTanim = new MenuTanimRepository(_context);
            //IKYS
            Personel = new PersonelRepository(_context);
            PersonelMenu = new PersonelMenuRepository(_context);
            IsBilgileri = new IsBilgileriRepository(_context);
            GorevTanim = new GorevTanimRepository(_context);
            BirimTanim = new BirimTanimRepository(_context);
            Kimlik = new KimlikRepository(_context);
            IletisimBilgileri = new IletisimBilgileriRepository(_context);
            ResmiTatil = new ResmiTatilRepository(_context);
            //TBYS
            TasinmazBagisci = new TasinmazBagisciRepository(_context);
            //NBYS
            NakitBagisci = new NakitBagisciRepository(_context);
            NakitBagisHareket = new NakitBagisHareketRepository(_context);
            //TYS
            Toplanti = new ToplantiRepository(_context);
            ToplantiKatilim = new ToplantiKatilimRepository(_context);
            //DYS
            EnvanterTanim = new EnvanterTanimRepository(_context);
            MalzemeGrubu = new MalzemeGrubuRepository(_context);
            MalzemeCinsi = new MalzemeCinsiRepository(_context);
            MarkaTanim = new MarkaTanimRepository(_context);
            ModelTanim = new ModelTanimRepository(_context);
            Ozellik = new OzellikRepository(_context);
            MalzemeOzellik = new MalzemeOzellikRepository(_context);
            Malzeme = new MalzemeRepository(_context);
            MalzemeYeriTanim = new MalzemeYeriTanimRepository(_context);
            MalzemeDagilim = new MalzemeDagilimRepository(_context);
            MalzemeHareket = new MalzemeHareketRepository(_context);
            Zimmet = new ZimmetRepository(_context);
        }

        //Common
        public IMenuItemRepository MenuItem { get; private set; }
        public IRolRepository Rol { get; private set; }
        public IRolMenuRepository RolMenu { get; private set; }
        public IPersonelRolRepository PersonelRol { get; private set; }

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
        
        public void Save()
        {
            _context.SaveChanges();
        }

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> SaveAsync(CancellationToken cancellationToken = default)
        {
            var changes = await _context.SaveChangesAsync(cancellationToken);
            return changes > 0;
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public ValueTask DisposeAsync()
        {
            return _context?.DisposeAsync() ?? ValueTask.CompletedTask;
        }
    }
}
