using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IKYS;
using Jenga.DataAccess.Repositories.IRepository;
using Jenga.DataAccess.Repositories.IRepository.IKYS;
using Jenga.DataAccess.Repositories.IRepository.TBYS;
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
        }

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
    }
}