using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.FTK;

namespace Jenga.DataAccess.Services.FTK
{
    public class FtkKisiService : IFtkKisiService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FtkKisiService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<List<FtkKisi>> GetAllAsync(CancellationToken cancellationToken = default)
            => _unitOfWork.FtkKisi.GetAllAsync(cancellationToken);

        public Task<FtkKisi?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => _unitOfWork.FtkKisi.GetByIdAsync(id, cancellationToken);

        public async Task<bool> AddAsync(FtkKisi model, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.FtkKisi.AddAsync(model, cancellationToken);
            await _unitOfWork.FtkKisi.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(FtkKisi model, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.FtkKisi.UpdateAsync(model, null, cancellationToken);
            await _unitOfWork.FtkKisi.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(FtkKisi model, CancellationToken cancellationToken = default)
        {
            _unitOfWork.FtkKisi.Remove(model);
            await _unitOfWork.FtkKisi.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
