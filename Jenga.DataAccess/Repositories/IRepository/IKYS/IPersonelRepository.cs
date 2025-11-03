using Jenga.Models.IKYS;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Jenga.DataAccess.Repositories.IRepository.IKYS
{
    public interface IPersonelRepository : IRepository<Personel>
    {
        Task SaveAsync(CancellationToken cancellationToken = default);

        Task<IEnumerable<SelectListItem>> GetPersonelDDL(bool onlyWorkingPersonel = true, int malzemeId = 0, CancellationToken cancellationToken = default);
    }
}