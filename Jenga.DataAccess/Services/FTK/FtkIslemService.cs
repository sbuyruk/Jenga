using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.FTK;

namespace Jenga.DataAccess.Services.FTK
{
    public class FtkIslemService : IFtkIslemService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FtkIslemService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<List<FtkIslem>> GetAllAsync(CancellationToken cancellationToken = default)
            => _unitOfWork.FtkIslem.GetAllAsync(cancellationToken);

        public Task<FtkIslem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => _unitOfWork.FtkIslem.GetByIdAsync(id, cancellationToken);

        public async Task<bool> AddAsync(FtkIslem model, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.FtkIslem.AddAsync(model, cancellationToken);
            await _unitOfWork.FtkIslem.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(FtkIslem model, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.FtkIslem.UpdateAsync(model, null, cancellationToken);
            await _unitOfWork.FtkIslem.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(FtkIslem model, CancellationToken cancellationToken = default)
        {
            _unitOfWork.FtkIslem.Remove(model);
            await _unitOfWork.FtkIslem.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
