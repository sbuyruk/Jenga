using Jenga.Models.MTS;

namespace Jenga.DataAccess.Repository.IRepository.MTS
{
    public interface IRandevuKatilimRepository : IRepository<RandevuKatilim>
    {
        void Update(RandevuKatilim obj);

    }
}
