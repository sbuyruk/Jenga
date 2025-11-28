using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.Models.Common;

namespace Jenga.DataAccess.Services.Menu
{
    public interface IPersonelRoleService
    {
        Task<List<PersonelRole>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PersonelRole?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(PersonelRole personelRole, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(PersonelRole personelRole, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(PersonelRole personelRole, CancellationToken cancellationToken = default);
        Task<IEnumerable<PersonelRole>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default);
    }
}
