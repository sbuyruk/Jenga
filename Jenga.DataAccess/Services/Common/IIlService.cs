using Jenga.Models.Common;
using Jenga.Utility.Results;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Services.Common
{
    public interface IIlService
    {
        Task<Result<List<Il>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<Il>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(Il il, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(Il il, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int ilId, CancellationToken cancellationToken = default);
        Task<Result<bool>> AnyAsync(Expression<Func<Il, bool>> predicate, CancellationToken cancellationToken = default);
        Task<Result<List<Il>>> GetAktifIllerAsync(CancellationToken cancellationToken = default);
    }
}
