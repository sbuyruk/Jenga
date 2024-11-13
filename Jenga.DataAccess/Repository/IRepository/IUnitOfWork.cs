using Jenga.DataAccess.Repository.IRepository.DYS;
using Jenga.DataAccess.Repository.IRepository.IKYS;
using Jenga.DataAccess.Repository.IRepository.MTS;
using Jenga.DataAccess.Repository.IRepository.NBYS;
using Jenga.DataAccess.Repository.IRepository.Ortak;
using Jenga.DataAccess.Repository.IRepository.TBYS;
using Jenga.DataAccess.Repository.IRepository.TYS;
using Jenga.Models.DYS;

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
        IAniObjesiDagitimRepository AniObjesiDagitim { get; }
        IMTSKurumTanimRepository MTSKurumTanim { get; }
        IMTSGorevTanimRepository MTSGorevTanim { get; }
        IMTSUnvanTanimRepository MTSUnvanTanim { get; }
        IMTSKurumGorevRepository MTSKurumGorev { get; }
        IFaaliyetKatilimRepository FaaliyetKatilim { get; }
        IFaaliyetRepository Faaliyet { get; }
        IFaaliyetAmaciRepository FaaliyetAmaci { get; }
        IAramaGorusmeRepository AramaGorusme { get; }
        //Ortak
        IBolgeRepository Bolge { get; }
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
        IKimlikRepository Kimlik { get; }
        IIletisimBilgileriRepository IletisimBilgileri { get; }
        IResmiTatilRepository ResmiTatil { get; }
        //TBYS
        ITasinmazBagisciRepository TasinmazBagisci { get; }
        //NBYS
        INakitBagisciRepository NakitBagisci { get; }
        INakitBagisHareketRepository NakitBagisHareket { get; }
        //TYS
        IToplantiRepository Toplanti { get; }
        IToplantiKatilimRepository ToplantiKatilim { get; }
        //DYS
        IEnvanterTanimRepository EnvanterTanim { get; }
        IMalzemeGrubuRepository MalzemeGrubu { get; }
        IMalzemeCinsiRepository MalzemeCinsi { get; }
        IMarkaTanimRepository MarkaTanim { get; }
        IModelTanimRepository ModelTanim { get; }
        IOzellikRepository Ozellik { get; }
        IMalzemeOzellikRepository MalzemeOzellik { get; }
        IMalzemeRepository Malzeme { get; }
        IMalzemeYeriTanimRepository MalzemeYeriTanim { get; }
        IMalzemeDagilimRepository MalzemeDagilim { get; }
        IMalzemeHareketRepository MalzemeHareket { get; }
        IZimmetRepository Zimmet{ get; }
        void Save();
        Task<int> CommitAsync();
    }
}
