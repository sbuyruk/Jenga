using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.FTK;

namespace Jenga.DataAccess.Services.FTK
{
    public class FtkService : IFtkService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FtkService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<List<Ftk>> GetAllAsync(CancellationToken cancellationToken = default)
            => _unitOfWork.Ftk.GetAllAsync(cancellationToken);

        public Task<List<Ftk>> GetLatestPerIslemAsync(CancellationToken cancellationToken = default)
            => _unitOfWork.Ftk.GetLatestPerIslemAsync(cancellationToken);

        public Task<Ftk?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => _unitOfWork.Ftk.GetByIdAsync(id, cancellationToken);

        public async Task<bool> AddAsync(Ftk model, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.Ftk.AddAsync(model, cancellationToken);
            await _unitOfWork.Ftk.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(Ftk model, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.Ftk.UpdateAsync(model, null, cancellationToken);
            await _unitOfWork.Ftk.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(Ftk model, CancellationToken cancellationToken = default)
        {
            _unitOfWork.Ftk.Remove(model);
            await _unitOfWork.Ftk.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
