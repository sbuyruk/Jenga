using Jenga.Models.MTS;

namespace Jenga.DataAccess.Repositories.IRepository.MTS
{
    public interface IFaaliyetKatilimRepository : IRepository<FaaliyetKatilim>
    {
        IEnumerable<FaaliyetKatilim> IncludeIt();

    }
}
