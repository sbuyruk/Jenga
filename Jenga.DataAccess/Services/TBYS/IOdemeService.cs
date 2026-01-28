using Jenga.Models.TBYS;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Services.TBYS
{
    public interface IOdemeService
    {
        Task<List<Odeme>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Odeme?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Odeme?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);
        Task<List<Odeme>> GetBySozlesmeIdAsync(int sozlesmeId, CancellationToken cancellationToken = default);
        Task<List<Odeme>> GetByKiraciIdAsync(int kiraciId, CancellationToken cancellationToken = default);
        Task<List<Odeme>> GetByOdemePlaniIdAsync(int odemePlaniId, CancellationToken cancellationToken = default);

        Task<bool> AddAsync(Odeme odeme, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(Odeme odeme, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int odemeId, CancellationToken cancellationToken = default);

        Task<bool> AnyAsync(Expression<Func<Odeme, bool>> predicate, CancellationToken cancellationToken = default);
        Task<(bool CanDelete, string? Reason)> CanDeleteAsync(int id);
    }
}
