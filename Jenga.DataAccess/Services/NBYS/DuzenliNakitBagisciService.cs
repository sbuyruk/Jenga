using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.NBYS;

namespace Jenga.DataAccess.Services.NBYS
{
    public class DuzenliNakitBagisciService : IDuzenliNakitBagisciService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DuzenliNakitBagisciService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<List<DuzenliNakitBagisci>> GetAllAsync(CancellationToken cancellationToken = default)
            => _unitOfWork.DuzenliNakitBagisci.GetAllAsync(cancellationToken);

        public Task<DuzenliNakitBagisci?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => _unitOfWork.DuzenliNakitBagisci.GetByIdAsync(id, cancellationToken);

        public async Task<bool> AddAsync(DuzenliNakitBagisci model, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.DuzenliNakitBagisci.AddAsync(model, cancellationToken);
            await _unitOfWork.DuzenliNakitBagisci.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(DuzenliNakitBagisci model, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.DuzenliNakitBagisci.UpdateAsync(model, null, cancellationToken);
            await _unitOfWork.DuzenliNakitBagisci.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(DuzenliNakitBagisci model, CancellationToken cancellationToken = default)
        {
            _unitOfWork.DuzenliNakitBagisci.Remove(model);
            await _unitOfWork.DuzenliNakitBagisci.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
