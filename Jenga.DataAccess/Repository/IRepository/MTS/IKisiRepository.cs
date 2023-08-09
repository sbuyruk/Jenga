using Jenga.Models.MTS;

namespace Jenga.DataAccess.Repository.IRepository.MTS
{
    public interface IKisiRepository : IRepository<Kisi>
    {
        void Update(Kisi obj);

    }
}
