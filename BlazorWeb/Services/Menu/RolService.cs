using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Common;

namespace Jenga.BlazorWeb.Services.Menu
{
    public class RolService : IRolService
    {
        private readonly IUnitOfWork _uow;

        public RolService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<List<Rol>> GetAllAsync()
            => (await _uow.Rol.GetAllAsync()).ToList();
        public Task<Rol?> GetByIdAsync(int id) => _uow.Rol.GetAsync(id);
        public async Task AddAsync(Rol rol)
        {
            await _uow.Rol.AddAsync(rol);
            await _uow.CommitAsync();
        }

        public async Task UpdateAsync(Rol rol)
        {
            _uow.Rol.Update(rol);
            await _uow.CommitAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var rol = await GetByIdAsync(id);
            if (rol != null)
            {
                _uow.Rol.Remove(rol);
                await _uow.CommitAsync();
            }
        }
    }

}
