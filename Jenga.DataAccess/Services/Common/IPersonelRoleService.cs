using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.Models.Common;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.Common
{
    public interface IPersonelRoleService
    {
        Task<Result<List<PersonelRole>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<PersonelRole>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(PersonelRole personelRole, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(PersonelRole personelRole, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(PersonelRole personelRole, CancellationToken cancellationToken = default);
        Task<Result<List<PersonelRole>>> GetByPersonelIdAsync(int personelId, CancellationToken cancellationToken = default);
    }
}
