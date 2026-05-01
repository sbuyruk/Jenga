using Jenga.Models.Inventory;
using Jenga.Utility.Results;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Inventory
{
    public interface IMaterialBrandService
    {
        Task<Result<List<MaterialBrand>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<MaterialBrand>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(MaterialBrand brand, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(MaterialBrand brand, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(MaterialBrand brand, CancellationToken cancellationToken = default);
        Task<Result<bool>> AnyAsync(Expression<Func<MaterialBrand, bool>> predicate, CancellationToken cancellationToken = default);
    }
}