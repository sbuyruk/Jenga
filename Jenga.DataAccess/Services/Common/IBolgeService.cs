using Jenga.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Services.Common
{
    public interface IBolgeService
    {
        Task<List<Bolge>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Bolge?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Bolge?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(Bolge bolge, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(Bolge bolge, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int bolgeId, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<Bolge, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
