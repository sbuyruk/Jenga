using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Common;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Services.Menu
{
    public class RolService : IRolService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RolService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<List<Rol>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _unitOfWork.Rol.GetAllAsync(cancellationToken);

        public async Task<Rol?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.Rol.GetByIdAsync(id, cancellationToken);

        public async Task<Rol?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default)
            => await _unitOfWork.Rol.GetByIdWithRelationsAsync(id, cancellationToken);

        public Task<bool> AnyAsync(Expression<Func<Rol, bool>> predicate)
            => _unitOfWork.Rol.AnyAsync(predicate);

        public async Task<bool> AddAsync(Rol rol, string? modifiedBy = null, CancellationToken cancellationToken = default)
        {
            if (rol == null) throw new ArgumentNullException(nameof(rol));

            // Repository.AddAsync in RolRepository does not commit; call SaveAsync after add.
            await _unitOfWork.Rol.AddAsync(rol, cancellationToken);
            await _unitOfWork.Rol.SaveAsync(cancellationToken);

            return true;
        }

        public async Task<bool> UpdateAsync(Rol rol, string? modifiedBy = null, CancellationToken cancellationToken = default)
        {
            if (rol == null) throw new ArgumentNullException(nameof(rol));

            // Repository.UpdateAsync will persist changes.
            await _unitOfWork.Rol.UpdateAsync(rol, modifiedBy, cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(Rol rol, CancellationToken cancellationToken = default)
        {
            if (rol == null) throw new ArgumentNullException(nameof(rol));

            // Repository.Remove is synchronous and commits immediately in current pattern.
            _unitOfWork.Rol.Remove(rol);
            return await Task.FromResult(true);
        }
    }
}