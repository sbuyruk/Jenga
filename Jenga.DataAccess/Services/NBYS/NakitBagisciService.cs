using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.NBYS;

namespace Jenga.DataAccess.Services.NBYS
{
    public class NakitBagisciService : INakitBagisciService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NakitBagisciService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<List<NakitBagisci>> GetAllAsync(CancellationToken cancellationToken = default)
            => _unitOfWork.NakitBagisci.GetAllAsync(cancellationToken);

        public Task<NakitBagisci?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => _unitOfWork.NakitBagisci.GetByIdAsync(id, cancellationToken);

        public async Task<bool> AddAsync(NakitBagisci model, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.NakitBagisci.AddAsync(model, cancellationToken);
            await _unitOfWork.NakitBagisci.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(NakitBagisci model, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.NakitBagisci.UpdateAsync(model, null, cancellationToken);
            await _unitOfWork.NakitBagisci.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(NakitBagisci model, CancellationToken cancellationToken = default)
        {
            _unitOfWork.NakitBagisci.Remove(model);
            await _unitOfWork.NakitBagisci.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
