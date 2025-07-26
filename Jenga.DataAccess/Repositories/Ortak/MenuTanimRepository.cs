using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Ortak;
using Jenga.Models.Ortak;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.Ortak
{
    public class MenuTanimRepository : Repository<MenuTanim>, IMenuTanimRepository
    {
        ApplicationDbContext _db;
        public MenuTanimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Save()
        {
            _db.SaveChanges();
        }

        public List<MenuTanimVM> GetSubMenuMenuListByParentId(int? parentId)
        {
            IQueryable<MenuTanim> query = _db.MenuTanim_Table;

            if (parentId != null)
            {
                query = query.Where(mt => mt.UstMenuId == parentId);
            }
            else
            {
                query = query.Where(mt => mt.UstMenuId > 0);
            }
            List<MenuTanim> menuList = query.ToList();
            List<MenuTanimVM> subMenu = new List<MenuTanimVM>();
            foreach (var menuItem in menuList)
            {

                MenuTanimVM menuTanimVM = new()
                {
                    MenuTanim = menuItem,

                };
                subMenu.Add(menuTanimVM);
                GetSubMenuMenuListByParentId(menuItem.Id);
            }
            return subMenu;
        }
        public List<MenuTanim> GetMenusByIds(IEnumerable<int> menuTanimIds)
        {
            return _db.Set<MenuTanim>()
                .Where(t => menuTanimIds.Contains(t.Id))
                .ToList();
        }

    }
}
