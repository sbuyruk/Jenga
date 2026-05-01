using Jenga.Models.Common;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.Common
{
    public interface IMenuItemService
    {
        Task<Result<List<MenuItem>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<MenuItem>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(MenuItem menuItem, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(MenuItem menuItem, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(MenuItem menuItem, CancellationToken cancellationToken = default);
        Task<Result<List<MenuItem>>> GetRecursiveMenuAsync();
        Task<Result<List<MenuItem>>> GetAuthorizedMenuAsync(int personelId);
    }
}