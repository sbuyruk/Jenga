using Jenga.Models.DYS;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Jenga.DataAccess.Repositories.IRepository.DYS
{
    public interface IMalzemeRepository : IRepository<Malzeme>
    {
        Task<IEnumerable<SelectListItem>> GetMalzemeDDL(bool onlyExistingMalzeme = false);
    }
}
