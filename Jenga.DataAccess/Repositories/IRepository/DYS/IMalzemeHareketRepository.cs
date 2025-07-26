using Jenga.Models.DYS;

namespace Jenga.DataAccess.Repositories.IRepository.DYS
{
    public interface IMalzemeHareketRepository : IRepository<MalzemeHareket>
    {
        IEnumerable<MalzemeHareket> GetMalzemeHareketWithDetails();

    }
}
