using Jenga.DataAccess.Repositories.IRepository.IKYS;
using Jenga.DataAccess.Repositories.IRepository.TBYS;

namespace Jenga.DataAccess.Repositories.IRepository
{
    public interface IUnitOfWork
    {
        //IKYS
        IPersonelRepository Personel { get; }
        IPersonelLocationRepository PersonelLocation { get; }
        IAileRepository Aile { get; }
        IDereceKademeDegisimRepository DereceKademeDegisim { get; }
        IEgitimSeviyesiRepository EgitimSeviyesi { get; }
        IGorevOnayRepository GorevOnay { get; }
        IBirimTanimRepository BirimTanim { get; }
        IGorevTanimRepository GorevTanim { get; }
        IIletisimBilgileriRepository IletisimBilgileri { get; }
        IIzinDonemRepository IzinDonem { get; }
        IIzinTalepRepository IzinTalep { get; }
        IIzinHareketRepository IzinHareket { get; }
        IIzinTanimRepository IzinTanim { get; }
        IKimlikRepository Kimlik { get; }
        IIsBilgileriRepository IsBilgileri { get; }
        IYabanciDilRepository YabanciDil { get; }
        ITahsilTanimRepository TahsilTanim { get; }

        //TBYS
        ITasinmazRepository Tasinmaz { get; }
        ITasinmazBagisciRepository TasinmazBagisci { get; }
        IBagisRepository Bagis { get; }
        IKiraciRepository Kiraci { get; }
        IKiraSozlesmeRepository KiraSozlesme { get; }
        ISozlesmeTasinmazRepository SozlesmeTasinmaz { get; }
        IOdemePlaniRepository OdemePlani { get; }
        IOdemeRepository Odeme { get; }
        IYasalFaizRepository YasalFaiz { get; }
        IBagisciTalepleriRepository BagisciTalepleri { get; }
        IBagisciYakinlariRepository BagisciYakinlari { get; }
        ITasinmazTaahhutRepository TasinmazTaahhut { get; }
        IVasiyetciRepository Vasiyetci { get; }
    }
}
