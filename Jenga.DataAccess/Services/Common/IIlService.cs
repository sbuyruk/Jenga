using Jenga.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Services.Common
{
    public interface IIlService
    {
        Task<List<Il>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Il?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(Il il, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(Il il, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int ilId, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<Il, bool>> predicate, CancellationToken cancellationToken = default);
        Task<List<Il>> GetAktifIllerAsync(CancellationToken cancellationToken = default);
    }
}
