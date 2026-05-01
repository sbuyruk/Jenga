using Jenga.Models.NBYS;
using Jenga.Utility.Results;

namespace Jenga.DataAccess.Services.NBYS
{
    public interface IBankaTanimService
    {
        Task<Result<List<BankaTanim>>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Result<BankaTanim>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result> AddAsync(BankaTanim model, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(BankaTanim model, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(BankaTanim model, CancellationToken cancellationToken = default);
    }
}
