using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.NBYS;

namespace Jenga.DataAccess.Services.NBYS
{
    public class ArmaganTanimService : IArmaganTanimService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ArmaganTanimService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<List<ArmaganTanim>> GetAllAsync(CancellationToken cancellationToken = default)
            => _unitOfWork.ArmaganTanim.GetAllAsync(cancellationToken);

        public Task<ArmaganTanim?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => _unitOfWork.ArmaganTanim.GetByIdAsync(id, cancellationToken);

        public async Task<bool> AddAsync(ArmaganTanim model, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.ArmaganTanim.AddAsync(model, cancellationToken);
            await _unitOfWork.ArmaganTanim.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(ArmaganTanim model, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.ArmaganTanim.UpdateAsync(model, null, cancellationToken);
            await _unitOfWork.ArmaganTanim.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(ArmaganTanim model, CancellationToken cancellationToken = default)
        {
            _unitOfWork.ArmaganTanim.Remove(model);
            await _unitOfWork.ArmaganTanim.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
