using Jenga.Models.TBYS;
using Jenga.Utility.Results;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.TBYS;

public interface IYasalFaizService
{
    Task<Result<List<YasalFaiz>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<YasalFaiz>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> AddAsync(YasalFaiz yasalFaiz, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(YasalFaiz yasalFaiz, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<bool>> AnyAsync(Expression<Func<YasalFaiz, bool>> predicate, CancellationToken cancellationToken = default);
}