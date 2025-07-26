using Jenga.Models.MTS;

namespace Jenga.DataAccess.Repositories.IRepository.MTS
{
    public interface IKisiRepository : IRepository<Kisi>
    {
        IEnumerable<Kisi> IncludeIt();
        Kisi IncludeThis(int? kisiId);

    }
}
