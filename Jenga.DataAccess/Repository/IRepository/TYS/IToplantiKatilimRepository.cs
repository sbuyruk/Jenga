using Jenga.Models.MTS;
using Jenga.Models.TYS;


namespace Jenga.DataAccess.Repository.IRepository.TYS
{
    public interface IToplantiKatilimRepository : IRepository<ToplantiKatilim>
    {
        void Update(ToplantiKatilim obj);

    }
}
