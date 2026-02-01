using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.Models.NBYS;

namespace Jenga.DataAccess.Services.NBYS
{
    public interface IBankaTanimService
    {
        Task<List<BankaTanim>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<BankaTanim?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> AddAsync(BankaTanim model, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(BankaTanim model, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(BankaTanim model, CancellationToken cancellationToken = default);
    }
}
