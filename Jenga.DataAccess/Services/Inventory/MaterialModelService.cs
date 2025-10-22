using Jenga.DataAccess.Repositories.IRepository;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialModelService : IMaterialModelService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MaterialModelService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<MaterialModel>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialModel.GetAllAsync(cancellationToken);

        public async Task<MaterialModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialModel.GetByIdAsync(id, cancellationToken);

        public async Task<bool> AddAsync(MaterialModel model, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.MaterialModel.AddAsync(model, cancellationToken);
            await _unitOfWork.MaterialModel.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(MaterialModel model, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.MaterialModel.UpdateAsync(model);
            await _unitOfWork.MaterialModel.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(MaterialModel model, CancellationToken cancellationToken = default)
        {
            _unitOfWork.MaterialModel.Remove(model);
            await _unitOfWork.MaterialModel.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}