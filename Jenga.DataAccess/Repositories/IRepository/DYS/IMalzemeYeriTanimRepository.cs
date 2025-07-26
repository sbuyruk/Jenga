using Jenga.Models.DYS;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Jenga.DataAccess.Repositories.IRepository.DYS
{
    public interface IMalzemeYeriTanimRepository : IRepository<MalzemeYeriTanim>
    {
        Task<IEnumerable<SelectListItem>> GetMalzemeYeriDDL(bool onlyExistingMalzeme = false, int malzemeId = 0);
    }
}
