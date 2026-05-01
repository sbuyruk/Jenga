using Jenga.Models.Common;
using Jenga.Utility.Results;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Services.Common
{
    public interface IBolgeService
    {
        Task<Result<List<Bolge>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<Bolge>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<Bolge>> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(Bolge bolge, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(Bolge bolge, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int bolgeId, CancellationToken cancellationToken = default);
        Task<Result<bool>> AnyAsync(Expression<Func<Bolge, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
