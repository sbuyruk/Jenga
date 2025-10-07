using Jenga.DataAccess.Repositories.IRepository;
using Jenga.DataAccess.Repositories.IRepository.Inventory;
using Jenga.Models.Inventory;
using System.Linq.Expressions;
using System.Threading;

namespace Jenga.DataAccess.Services.Inventory
{
    public class MaterialEntryService : IMaterialEntryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MaterialEntryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<MaterialEntry>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialEntry.GetAllAsync(cancellationToken);

        public async Task<MaterialEntry?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.MaterialEntry.GetByIdAsync(id, cancellationToken);

        public async Task<bool> AddAsync(MaterialEntry entry, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.MaterialEntry.AddAsync(entry, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(MaterialEntry entry, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.MaterialEntry.UpdateAsync(entry);
            await _unitOfWork.SaveAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(MaterialEntry entry, CancellationToken cancellationToken = default)
        {
            _unitOfWork.MaterialEntry.Remove(entry);
            await _unitOfWork.SaveAsync(cancellationToken);
            return true;
        }



        public Task<bool> AnyAsync(Expression<Func<MaterialEntry, bool>> predicate)
        {
            return _unitOfWork.MaterialEntry.AnyAsync(predicate);
        }
    }
}