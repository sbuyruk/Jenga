using Jenga.Models.Ortak;

namespace Jenga.DataAccess.Repositories.IRepository.Ortak
{
    public interface IPersonelMenuRepository : IRepository<PersonelMenu>
    {
        List<PersonelMenu> GetPersonelMenuByPersonelId(int? personnelId);

    }
}
