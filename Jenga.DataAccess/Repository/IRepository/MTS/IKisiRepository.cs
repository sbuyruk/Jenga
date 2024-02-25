using Jenga.Models.MTS;

namespace Jenga.DataAccess.Repository.IRepository.MTS
{
    public interface IKisiRepository : IRepository<Kisi>
    {
        IEnumerable<Kisi> IncludeIt();
        Kisi IncludeThis(int? kisiId);
        void Update(Kisi obj);

    }
}
