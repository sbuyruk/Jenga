using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Common;
using Jenga.Utility.Logging;

namespace Jenga.DataAccess.Services.Menu
{
    public class MenuItemService : IMenuItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogService _logService;

        public MenuItemService(IUnitOfWork unitOfWork, ILogService logService)
        {
            _unitOfWork = unitOfWork;
            _logService = logService;
        }

        public async Task<List<MenuItem>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var menuItems = await _unitOfWork.MenuItem.GetAllAsync();
                return menuItems.ToList();
            }
            catch (Exception ex)
            {
                _logService.LogError("MenuItemService.GetAllAsync", ex);
                return new();
            }
        }

        public async Task<MenuItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.MenuItem.GetByIdAsync(id, cancellationToken);
        }

        public async Task<bool> AddAsync(MenuItem menuItem, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.MenuItem.AddAsync(menuItem, cancellationToken);
            return await _unitOfWork.SaveAsync(cancellationToken);
        }

        public async Task<bool> UpdateAsync(MenuItem menuItem, CancellationToken cancellationToken = default)
        {
            _unitOfWork.MenuItem.Update(menuItem);
            return await _unitOfWork.SaveAsync(cancellationToken);
        }

        public async Task<bool> DeleteAsync(MenuItem menuItem, CancellationToken cancellationToken = default)
        {
            if (menuItem is null) return false;

            _unitOfWork.MenuItem.Remove(menuItem);
            return await _unitOfWork.SaveAsync(cancellationToken);
        }
    }
}
