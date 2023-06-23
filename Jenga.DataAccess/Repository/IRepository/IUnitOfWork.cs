using Jenga.DataAccess.Repository.IRepository.IKYS;
using Jenga.DataAccess.Repository.IRepository.MTS;
using Jenga.DataAccess.Repository.IRepository.Ortak;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        IDepoTanimRepository DepoTanim { get; }
        IKaynakTanimRepository KaynakTanim { get; }
        IAniObjesiTanimRepository AniObjesiTanim { get; }
        IDepoHareketRepository DepoHareket { get; }
        IDepoStokRepository DepoStok { get; }
        IModulTanimRepository ModulTanim { get; }
        IMenuTanimRepository MenuTanim { get; }
        IPersonelRepository Personel { get; }
        IPersonelMenuRepository PersonelMenu { get; }
        IIsBilgileriRepository IsBilgileri { get; }
        IGorevTanimRepository GorevTanim { get; }
        IBirimTanimRepository BirimTanim { get; }
        IUnvanTanimRepository UnvanTanim { get; }
        IDagitimYeriTanimRepository DagitimYeriTanim { get; }
        void Save();
    }
}
