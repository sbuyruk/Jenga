using Jenga.DataAccess.Repositories;
using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Common;
using Jenga.Utility.Helpers;

namespace Jenga.BlazorUI.Services.Menu
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
            // 1. Tüm öğeleri çek
            var flat = await _unitOfWork.MenuItem.GetAllAsync();

            // 2. Görünür olanları filtrele
            var visible = flat.Where(m => m.IsVisible==true).ToList();

            // 3. Url null ya da boşsa default ataması yap
            visible.ForEach(m =>
                m.Url = string.IsNullOrWhiteSpace(m.Url)
                        ? "#"
                        : m.Url!
            );

            // 4. Ağaç yapısını kurup dön
            return MenuHelper.BuildTree(visible);
        }

    }

}