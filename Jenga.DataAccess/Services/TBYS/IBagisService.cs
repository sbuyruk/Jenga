using Jenga.Models.TBYS;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Services.TBYS
{
    public interface IBagisService
    {
        Task<List<Bagis>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Bagis?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Bagis?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);
        Task<List<Bagis>> GetByBagisciIdAsync(int bagisciId, CancellationToken cancellationToken = default);
        Task<List<Bagis>> GetByTasinmazIdAsync(int tasinmazId, CancellationToken cancellationToken = default);

        Task<bool> AddAsync(Bagis bagis, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(Bagis bagis, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int bagisId, CancellationToken cancellationToken = default);

        Task<bool> AnyAsync(Expression<Func<Bagis, bool>> predicate, CancellationToken cancellationToken = default);
        Task<(bool CanDelete, string? Reason)> CanDeleteAsync(int id);
    }
}