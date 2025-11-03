using Jenga.Models.Ortak;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Jenga.DataAccess.Repositories.IRepository.Ortak
{
    public interface IIlRepository : IRepository<Il>
    {
        Task SaveAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<SelectListItem>> GetIlDDL(int bolgeId = 0, CancellationToken cancellationToken = default);
    }
}