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
            List<MenuTanim> menuTanimList =GetByFilter(u => u.UstMenuId == ustMenuId);
            
            foreach (var item in menuTanimList)
            {
                List<MenuTree> subMenuList = new List<MenuTree>();

                MenuTree menuTree = new()
                {
                    text = item.Adi,
                    id = item.Id,
                    nodes = MenuTreeJsonGetir(subMenuList, item.Id)
                };
                menuTreeList.Add(menuTree);
            }
            
            return menuTreeList;
        }
        public string  GetMenuAll(int ustMenuId)
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
                objFromDb.UstMenuId = obj.UstMenuId;
                objFromDb.Adi = obj.Adi;
                objFromDb.Aciklama = obj.Aciklama;
                objFromDb.Degistiren = obj.Degistiren;
                objFromDb.DegistirmeTarihi = obj.DegistirmeTarihi;

            }
        }
    }
}
