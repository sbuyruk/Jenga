using global::Jenga.DataAccess.Repositories.IRepository;
using global::Jenga.Utility.Logging;
using Jenga.Models.Common;

namespace Jenga.DataAccess.Services.Menu
{
    public class RolService : IRolService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogService _logService;

        public RolService(IUnitOfWork unitOfWork, ILogService logService)
        {
            _unitOfWork = unitOfWork;
            _logService = logService;
        }

        public async Task<List<Rol>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var roller = await _unitOfWork.Rol.GetAllAsync(cancellationToken);
                var list = roller.ToList();
                return list;
            }
            catch (Exception ex)
            {
                _logService.LogError("RolService.GetAllAsync", ex);
                return new();
            }
            //try
            //{
            //    return await _unitOfWork.Rol.GetAllAsync(cancellationToken);
            //}
            //catch (Exception ex)
            //{
            //    _logService.LogError("RolService.GetAllAsync", ex);
            //    return new();
            //}
        }

        public async Task<Rol?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.Rol.GetByIdAsync(id, cancellationToken);
        }

        public async Task<bool> AddAsync(Rol rol, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.Rol.AddAsync(rol, cancellationToken);
            await _unitOfWork.Rol.SaveChangesAsync(cancellationToken);
            return true;
        }


        public async Task<bool> UpdateAsync(Rol rol, CancellationToken cancellationToken = default)
        {
            _unitOfWork.Rol.Update(rol);
            await _unitOfWork.Rol.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(Rol rol, CancellationToken cancellationToken = default)
        {
            if (rol is null) return false;

            _unitOfWork.Rol.Remove(rol);
            await _unitOfWork.Rol.SaveChangesAsync(cancellationToken);
            return true;
        }
        public async Task<Rol?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.Rol.GetByIdWithRelationsAsync(id, cancellationToken);
        }
    }

}
