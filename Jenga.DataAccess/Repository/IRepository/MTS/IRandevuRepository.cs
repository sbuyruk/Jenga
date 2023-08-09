using Jenga.Models.MTS;

namespace Jenga.DataAccess.Repository.IRepository.MTS
{
    public interface IRandevuRepository : IRepository<Randevu>
    {
        void Update(Randevu obj);

    }
}
