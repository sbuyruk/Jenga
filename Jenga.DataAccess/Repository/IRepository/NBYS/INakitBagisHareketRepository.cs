using Jenga.Models.NBYS;
using Jenga.Models.TBYS;

namespace Jenga.DataAccess.Repository.IRepository.NBYS
{
    public interface INakitBagisHareketRepository : IRepository<NakitBagisHareket>
    {
        bool Update(NakitBagisHareket obj);

    }
}
