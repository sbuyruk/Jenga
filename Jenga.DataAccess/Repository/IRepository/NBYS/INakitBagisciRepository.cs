using Jenga.Models.NBYS;
using Jenga.Models.TBYS;

namespace Jenga.DataAccess.Repository.IRepository.NBYS
{
    public interface INakitBagisciRepository : IRepository<NakitBagisci>
    {
        bool Update(NakitBagisci obj);

    }
}
