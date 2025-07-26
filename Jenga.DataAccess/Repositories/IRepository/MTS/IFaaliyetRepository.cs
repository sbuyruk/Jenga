using Jenga.Models.MTS;

namespace Jenga.DataAccess.Repositories.IRepository.MTS
{
    public interface IFaaliyetRepository : IRepository<Faaliyet>
    {
        IEnumerable<Faaliyet> IncludeIt(DateTime? baslangicTarihi);

    }
}
