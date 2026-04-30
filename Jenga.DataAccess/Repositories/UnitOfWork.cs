using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.Common;
using Jenga.DataAccess.Repositories.IKYS;
using Jenga.DataAccess.Repositories.IRepository;
using Jenga.DataAccess.Repositories.IRepository.Common;
using Jenga.DataAccess.Repositories.IRepository.IKYS;
using Jenga.DataAccess.Repositories.IRepository.FTK;
using Jenga.DataAccess.Repositories.IRepository.NBYS;
using Jenga.DataAccess.Repositories.IRepository.TBYS;
using Jenga.DataAccess.Repositories.FTK;
using Jenga.DataAccess.Repositories.NBYS;
using Jenga.DataAccess.Repositories.TBYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public UnitOfWork(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;

            MenuItem = new MenuItemRepository(_contextFactory);
            Role = new RoleRepository(_contextFactory);
            RoleMenu = new RoleMenuRepository(_contextFactory);
            PersonelRole = new PersonelRoleRepository(_contextFactory);

            // Ortak
            Bolge = new BolgeRepository(_contextFactory);
            Il = new IlRepository(_contextFactory);
            Ilce = new IlceRepository(_contextFactory);

            // IKYS
            Personel = new PersonelRepository(_contextFactory);
            PersonelLocation = new PersonelLocationRepository(_contextFactory);
            Aile = new AileRepository(_contextFactory);
            DereceKademeDegisim = new DereceKademeDegisimRepository(_contextFactory);
            EgitimSeviyesi = new EgitimSeviyesiRepository(_contextFactory);
            GorevOnay = new GorevOnayRepository(_contextFactory);
            BirimTanim = new BirimTanimRepository(_contextFactory);
            GorevTanim = new GorevTanimRepository(_contextFactory);
            IletisimBilgileri = new IletisimBilgileriRepository(_contextFactory);
            IzinDonem = new IzinDonemRepository(_contextFactory);
            IzinTalep = new IzinTalepRepository(_contextFactory);
            IzinHareket = new IzinHareketRepository(_contextFactory);
            IzinTanim = new IzinTanimRepository(_contextFactory);
            Kimlik = new KimlikRepository(_contextFactory);
            IsBilgileri = new IsBilgileriRepository(_contextFactory);
            YabanciDil = new YabanciDilRepository(_contextFactory);
            TahsilTanim = new TahsilTanimRepository(_contextFactory);

            // TBYS
            Tasinmaz = new TasinmazRepository(_contextFactory);
            TasinmazBagisci = new TasinmazBagisciRepository(_contextFactory);
            Bagis = new BagisRepository(_contextFactory);
            Kiraci = new KiraciRepository(_contextFactory);
            KiraSozlesme = new KiraSozlesmeRepository(_contextFactory);
            SozlesmeTasinmaz = new SozlesmeTasinmazRepository(_contextFactory);
            OdemePlani = new OdemePlaniRepository(_contextFactory);
            Odeme = new OdemeRepository(_contextFactory);
            YasalFaiz = new YasalFaizRepository(_contextFactory);
            BagisciTalepleri = new BagisciTalepleriRepository(_contextFactory);
            BagisciYakinlari = new BagisciYakinlariRepository(_contextFactory);
            TasinmazTaahhut = new TasinmazTaahhutRepository(_contextFactory);
            Vasiyetci = new VasiyetciRepository(_contextFactory);

            // NBYS
            NakitBagisci = new NakitBagisciRepository(_contextFactory);
            NakitBagisHareket = new NakitBagisHareketRepository(_contextFactory);
            Armagan = new ArmaganRepository(_contextFactory);
            BankaTanim = new BankaTanimRepository(_contextFactory);
            ArmaganTanim = new ArmaganTanimRepository(_contextFactory);
            DuzenliNakitBagisci = new DuzenliNakitBagisciRepository(_contextFactory);

            // FTK
            Ftk = new FtkRepository(_contextFactory);
            FtkIslem = new FtkIslemRepository(_contextFactory);
            FtkKisi = new FtkKisiRepository(_contextFactory);
        }

        // Common
        public IMenuItemRepository MenuItem { get; private set; }
        public IRoleRepository Role { get; private set; }
        public IRoleMenuRepository RoleMenu { get; private set; }
        public IPersonelRoleRepository PersonelRole { get; private set; }
        public IBolgeRepository Bolge { get; private set; }
        public IIlRepository Il { get; private set; }
        public IIlceRepository Ilce { get; private set; }

        // IKYS
        public IPersonelRepository Personel { get; private set; }
        public IPersonelLocationRepository PersonelLocation { get; private set; }
        public IAileRepository Aile { get; private set; }
        public IDereceKademeDegisimRepository DereceKademeDegisim { get; private set; }
        public IEgitimSeviyesiRepository EgitimSeviyesi { get; private set; }
        public IGorevOnayRepository GorevOnay { get; private set; }
        public IBirimTanimRepository BirimTanim { get; private set; }
        public IGorevTanimRepository GorevTanim { get; private set; }
        public IIletisimBilgileriRepository IletisimBilgileri { get; private set; }
        public IIzinDonemRepository IzinDonem { get; private set; }
        public IIzinTalepRepository IzinTalep { get; private set; }
        public IIzinHareketRepository IzinHareket { get; private set; }
        public IIzinTanimRepository IzinTanim { get; private set; }
        public IKimlikRepository Kimlik { get; private set; }
        public IIsBilgileriRepository IsBilgileri { get; private set; }
        public IYabanciDilRepository YabanciDil { get; private set; }
        public ITahsilTanimRepository TahsilTanim { get; private set; }

        // TBYS
        public ITasinmazRepository Tasinmaz { get; private set; }
        public ITasinmazBagisciRepository TasinmazBagisci { get; private set; }
        public IBagisRepository Bagis { get; private set; }
        public IKiraciRepository Kiraci { get; private set; }
        public IKiraSozlesmeRepository KiraSozlesme { get; private set; }
        public ISozlesmeTasinmazRepository SozlesmeTasinmaz { get; private set; }
        public IOdemePlaniRepository OdemePlani { get; private set; }
        public IOdemeRepository Odeme { get; private set; }
        public IYasalFaizRepository YasalFaiz { get; private set; }
        public IBagisciTalepleriRepository BagisciTalepleri { get; private set; }
        public IBagisciYakinlariRepository BagisciYakinlari { get; private set; }
        public ITasinmazTaahhutRepository TasinmazTaahhut { get; private set; }
        public IVasiyetciRepository Vasiyetci { get; private set; }

        // NBYS
        public INakitBagisciRepository NakitBagisci { get; private set; }
        public INakitBagisHareketRepository NakitBagisHareket { get; private set; }
        public IArmaganRepository Armagan { get; private set; }
        public IBankaTanimRepository BankaTanim { get; private set; }
        public IArmaganTanimRepository ArmaganTanim { get; private set; }
        public IDuzenliNakitBagisciRepository DuzenliNakitBagisci { get; private set; }

        // FTK
        public IFtkRepository Ftk { get; private set; }
        public IFtkIslemRepository FtkIslem { get; private set; }
        public IFtkKisiRepository FtkKisi { get; private set; }
    }
}