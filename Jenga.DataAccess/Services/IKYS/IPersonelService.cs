using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Jenga.Models;
using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Services.IKYS
{
    public interface IPersonelService
    {
        Task<List<Personel>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Personel?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(Personel personel, string? modifiedBy = null, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(Personel personel, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Personel personel, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<Personel, bool>> predicate);
        Task<bool> UpdatePersonelAndSaveAsync(Personel personel, string? currentUserName, CancellationToken cancellationToken = default);
        Task<bool> DeletePersonelAndSaveAsync(Personel personel, string? currentUserName, CancellationToken cancellationToken = default);
        Task<List<Personel>> GetCalisanPersonelAsync(CancellationToken cancellationToken = default);
        Task<List<Personel>> GetKadroluPersonelAsync(CancellationToken cancellationToken = default);
    }
}