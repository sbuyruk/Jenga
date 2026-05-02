using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Jenga.Models;
using Jenga.Models.IKYS;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.IKYS
{
    public interface IPersonelService
    {
        Task<Result<List<Personel>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<Personel>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(Personel personel, string? modifiedBy = null, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(Personel personel, string? modifiedBy = null, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(Personel personel, CancellationToken cancellationToken = default);
        Task<Result<bool>> AnyAsync(Expression<Func<Personel, bool>> predicate, CancellationToken cancellationToken = default);
        Task<Result> UpdatePersonelAndSaveAsync(Personel personel, string? currentUserName, CancellationToken cancellationToken = default);
        Task<Result> DeletePersonelAndSaveAsync(Personel personel, string? currentUserName, CancellationToken cancellationToken = default);
        Task<Result<List<Personel>>> GetCalisanPersonelAsync(CancellationToken cancellationToken = default);
        Task<Result<List<Personel>>> GetKadroluPersonelAsync(CancellationToken cancellationToken = default);
    }
}