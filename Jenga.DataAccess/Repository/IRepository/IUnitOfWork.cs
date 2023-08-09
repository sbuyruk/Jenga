using Jenga.DataAccess.Repository.IRepository.IKYS;
using Jenga.DataAccess.Repository.IRepository.MTS;
using Jenga.DataAccess.Repository.IRepository.Ortak;

namespace Jenga.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        //MTS
        IDepoTanimRepository DepoTanim { get; }
        IKaynakTanimRepository KaynakTanim { get; }
        IAniObjesiTanimRepository AniObjesiTanim { get; }
        IDepoHareketRepository DepoHareket { get; }
        IDepoStokRepository DepoStok { get; }
        IDagitimYeriTanimRepository DagitimYeriTanim { get; }
        IGonderiPaketiRepository GonderiPaketi { get; }
        IKisiRepository Kisi { get; }
        IRandevuRepository Randevu { get; }
        IRandevuKatilimRepository RandevuKatilim { get; }
        IAniObjesiDagitimRepository AniObjesiDagitim { get; }
        //Ortak
        IIlRepository Il { get; }
        IIlceRepository Ilce { get; }
        IModulTanimRepository ModulTanim { get; }
        IMenuTanimRepository MenuTanim { get; }
        //IKYS
        IPersonelRepository Personel { get; }
        IPersonelMenuRepository PersonelMenu { get; }
        IIsBilgileriRepository IsBilgileri { get; }
        IGorevTanimRepository GorevTanim { get; }
        IBirimTanimRepository BirimTanim { get; }
        IUnvanTanimRepository UnvanTanim { get; }
        void Save();
    }
}
