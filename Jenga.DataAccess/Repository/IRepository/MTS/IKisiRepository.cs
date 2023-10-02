using Jenga.Models.MTS;

namespace Jenga.DataAccess.Repository.IRepository.MTS
{
    public interface IKisiRepository : IRepository<Kisi>
    {
        IEnumerable<Kisi> IncludeIt();
        void Update(Kisi obj);

    }
}
