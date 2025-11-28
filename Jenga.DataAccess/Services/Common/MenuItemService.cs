using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Common;
using Jenga.Utility.Helpers;
using Jenga.Utility.Logging;

namespace Jenga.DataAccess.Services.Common
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

        public async Task<List<Models.Common.MenuItem>> GetAllAsync(CancellationToken cancellationToken = default)
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

        public async Task<Models.Common.MenuItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.MenuItem.GetByIdAsync(id, cancellationToken);
        }

        public async Task<bool> AddAsync(Models.Common.MenuItem menuItem, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.MenuItem.AddAsync(menuItem, cancellationToken);
            await _unitOfWork.MenuItem.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(Models.Common.MenuItem menuItem, CancellationToken cancellationToken = default)
        {
            _unitOfWork.MenuItem.Update(menuItem);
            await _unitOfWork.MenuItem.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(Models.Common.MenuItem menuItem, CancellationToken cancellationToken = default)
        {
            if (menuItem is null) return false;

            _unitOfWork.MenuItem.Remove(menuItem);
            await _unitOfWork.MenuItem.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<List<MenuItem>> GetRecursiveMenuAsync()
        {
            var flat = await _unitOfWork.MenuItem.GetAllAsync();
            var visible = flat.Where(m => m.IsVisible == true).ToList();

            visible.ForEach(m =>
                m.Url = string.IsNullOrWhiteSpace(m.Url) ? "#" : m.Url!
            );

            return MenuHelper.BuildTree(visible);
        }

        public async Task<List<MenuItem>> GetAuthorizedMenuAsync(int personelId)
        {
            // 1. Personelin rollerini al
            var roles = await _unitOfWork.PersonelRole
                .GetAllByFilterAsync(x => x.PersonelId == personelId);
            var roleIds = roles.Select(r => r.RoleId).ToList();

            await Task.Delay(1); // context flush için mikro gecikme

            // 2. Rollerle ilişkilendirilmiş menü ID'lerini al
            var roleMenus = await _unitOfWork.RoleMenu
                .GetAllByFilterAsync(x => roleIds.Contains(x.RoleId));
            var menuIds = roleMenus.Select(rm => rm.MenuId).ToList();

            await Task.Delay(1); // context flush için mikro gecikme

            // 3. İlgili ve görünür menü öğelerini al
            var allMenus = await _unitOfWork.MenuItem
                .GetAllByFilterAsync(x => menuIds.Contains(x.Id) && x.IsVisible == true);
            var processedMenus = allMenus
                .Select(m =>
                {
                    m.Url = string.IsNullOrWhiteSpace(m.Url) ? "#" : m.Url!;
                    return m;
                }).ToList();

            // 4. Menü ağaç yapısını oluştur
            return MenuHelper.BuildTree(processedMenus);
        }
    }
}
