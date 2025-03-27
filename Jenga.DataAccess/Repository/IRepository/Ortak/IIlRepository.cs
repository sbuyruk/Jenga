using Jenga.Models.Ortak;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Jenga.DataAccess.Repository.IRepository.Ortak
{
    public interface IIlRepository : IRepository<Il>
    {
        Task<IEnumerable<SelectListItem>> GetIlDDL(int bolgeId = 0);
    }
}
