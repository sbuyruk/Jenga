using Jenga.Models.TBYS;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS;

public interface IYasalFaizService
{
    Task<List<YasalFaiz>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<YasalFaiz?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> AddAsync(YasalFaiz yasalFaiz, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(YasalFaiz yasalFaiz, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<YasalFaiz, bool>> predicate, CancellationToken cancellationToken = default);
}