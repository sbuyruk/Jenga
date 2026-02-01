using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.NBYS;

namespace Jenga.DataAccess.Services.NBYS
{
    public class BankaTanimService : IBankaTanimService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BankaTanimService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<List<BankaTanim>> GetAllAsync(CancellationToken cancellationToken = default)
            => _unitOfWork.BankaTanim.GetAllAsync(cancellationToken);

        public Task<BankaTanim?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => _unitOfWork.BankaTanim.GetByIdAsync(id, cancellationToken);

        public async Task<bool> AddAsync(BankaTanim model, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.BankaTanim.AddAsync(model, cancellationToken);
            await _unitOfWork.BankaTanim.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(BankaTanim model, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.BankaTanim.UpdateAsync(model, null, cancellationToken);
            await _unitOfWork.BankaTanim.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(BankaTanim model, CancellationToken cancellationToken = default)
        {
            _unitOfWork.BankaTanim.Remove(model);
            await _unitOfWork.BankaTanim.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
