using Jenga.Models.TBYS;
using Jenga.Utility.Results;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Services.TBYS
{
    public interface IBagisService
    {
        Task<Result<List<Bagis>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<List<BagisTasinmazItem>>> GetAllForArmaganDashboardAsync(CancellationToken cancellationToken = default);
        Task<Result<List<Bagis>>> GetAllEnvanterdeAsync(CancellationToken cancellationToken = default);
        Task<Result<Bagis>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<Bagis>> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<List<Bagis>>> GetByBagisciIdAsync(int bagisciId, CancellationToken cancellationToken = default);
        Task<Result<List<Bagis>>> GetByTasinmazIdAsync(int tasinmazId, CancellationToken cancellationToken = default);

        Task<Result> AddAsync(Bagis bagis, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(Bagis bagis, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int bagisId, CancellationToken cancellationToken = default);

        Task<Result<bool>> AnyAsync(Expression<Func<Bagis, bool>> predicate, CancellationToken cancellationToken = default);
        Task<Result<(bool CanDelete, string? Reason)>> CanDeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}