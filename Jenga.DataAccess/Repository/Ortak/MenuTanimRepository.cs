using Jenga.DataAccess.Data;
using Jenga.Models.Ortak;
using Jenga.DataAccess.Repository.IRepository.Ortak;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Linq.Expressions;
using Jenga.DataAccess.Repository.IRepository;

namespace Jenga.DataAccess.Repository.Ortak
{
    public class MenuTanimRepository : Repository<MenuTanim>, IMenuTanimRepository
    {
        ApplicationDbContext _db;
        public MenuTanimRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        
        private List<MenuTree> MenuTreeJsonGetir(List<MenuTree> menuTreeList, int ustMenuId)
        {
            List<MenuTanim> menuTanimList = GetByFilter(u => u.UstMenuId == ustMenuId);
            var menuTanimSortedList = menuTanimList.OrderBy(x => x.Sira).ThenBy(x => x.UstMenuId);
            foreach (var item in menuTanimSortedList)
            {
                List<MenuTree> subMenuList = new List<MenuTree>();

                MenuTree menuTree = new()
                {
                    id = item.Id,
                    text = item.Adi,
                    url=item.Url,
                    webpart=item.Webpart,
                    sira=item.Sira,
                    aciklama=item.Aciklama,
                    nodes = MenuTreeJsonGetir(subMenuList, item.Id)
                };
                menuTreeList.Add(menuTree);
            }
            
            return menuTreeList;
        }

        public string  GetMenuJson(int ustMenuId)
        {
            try
            {
                List<MenuTree> menuTreeList = new List<MenuTree>();
                menuTreeList= MenuTreeJsonGetir(menuTreeList, ustMenuId);
                string json = JsonSerializer.Serialize(menuTreeList);
                
                return json;
                
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }
        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(MenuTanim obj)
        {
            var objFromDb = _db.MenuTanim_Table.FirstOrDefault(u => u.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.Adi = obj.Adi;
                objFromDb.UstMenuId = obj.UstMenuId;
                objFromDb.Url = obj.Url;
                objFromDb.Webpart = obj.Webpart;
                objFromDb.Sira = obj.Sira;
                objFromDb.Aciklama = obj.Aciklama;
                objFromDb.Degistiren = obj.Degistiren;
                objFromDb.DegistirmeTarihi = obj.DegistirmeTarihi;

            }
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
            List<MenuTanim> menuList=query.ToList() ;
            List<MenuTanimVM> subMenu=new List<MenuTanimVM>() ;
            foreach (var menuItem in menuList) {
                
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
