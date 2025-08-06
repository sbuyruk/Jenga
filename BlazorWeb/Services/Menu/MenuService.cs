using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Common;
using Jenga.Utility.Helpers;
using System.Linq;

namespace Jenga.BlazorWeb.Services.Menu
{
    public class MenuService : IMenuService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MenuService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<MenuItem>> GetRecursiveMenuAsync()
        {
            var flat = await _unitOfWork.MenuItem.GetAllAsync();
            var visible = flat.Where(m => m.IsVisible==true).ToList();

            visible.ForEach(m =>
                m.Url = string.IsNullOrWhiteSpace(m.Url) ? "#" : m.Url!
            );

            return MenuHelper.BuildTree(visible);
        }

        public async Task<List<MenuItem>> GetAuthorizedMenuAsync(int personelId)
        {
            // 1. Personelin rollerini al
            var roles = await _unitOfWork.PersonelRol
                .GetAllByFilterAsync(x => x.PersonelId == personelId);
            var roleIds = roles.Select(r => r.RolId).ToList();

            // 2. Rollerle ilişkilendirilmiş menü ID'lerini al
            var roleMenus = await _unitOfWork.RolMenu
                .GetAllByFilterAsync(x => roleIds.Contains(x.RolId));
            var menuIds = roleMenus.Select(rm => rm.MenuId).ToList();

            // 3. İlgili ve görünür menü öğelerini al
            var allMenus = await _unitOfWork.MenuItem
                .GetAllByFilterAsync(x => menuIds.Contains(x.Id) && x.IsVisible == true);
            var processedMenus = allMenus
                .Select(m => {
                    m.Url = string.IsNullOrWhiteSpace(m.Url) ? "#" : m.Url!;
                    return m;
                }).ToList();

            // 4. Menü ağaç yapısını oluştur
            return MenuHelper.BuildTree(processedMenus);
        }
    }
}
