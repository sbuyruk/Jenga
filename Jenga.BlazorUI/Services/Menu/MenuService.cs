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
            var flat = await _unitOfWork.MenuItem.GetAllAsync();
            var visible = flat.Where(m => m.IsVisible==true).ToList();
            return MenuHelper.BuildTree(visible);
        }

        //public async Task<List<MenuItem>> GetRecursiveMenuAsync()
        //{
        //    var allMenuItems = await _unitOfWork.MenuItem.GetAllAsync();
        //    var visibleItems = allMenuItems
        //        .Where(m => m.IsVisible == true)
        //        .OrderBy(m => m.DisplayOrder)
        //        .ToList();



        //    return BuildHierarchy(visibleItems);
        //}

        //private List<MenuItem> BuildHierarchy(List<MenuItem> items)
        //{
        //    var lookup = items.ToLookup(item => item.ParentId);
        //    foreach (var item in items)
        //        item.Children = lookup[item.Id].ToList();

        //    return lookup[null].ToList();
        //}

    }

}