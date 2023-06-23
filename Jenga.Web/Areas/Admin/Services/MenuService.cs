using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.IKYS;
using Jenga.Models.Ortak;
using System.Text.Json;

namespace Jenga.Web.Areas.Admin.Services
{
    public class MenuService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MenuService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns>MenuTanimVM listesi</returns>
        public List<MenuTanimVM> GetMenuTanimVmListByParent(int parentId)
        {
            List<MenuTanim> menuTanims = _unitOfWork.MenuTanim.GetByFilter(u => u.UstMenuId == parentId);
            List<MenuTanimVM> menuTanimVMs = new List<MenuTanimVM>();

            foreach (var menuTanim in menuTanims)
            {
                MenuTanimVM menuTanimVM = new MenuTanimVM
                {
                    MenuTanim = menuTanim,
                    SubMenu = GetSubMenu(menuTanim.Id),
                    UstMenuTanim = _unitOfWork.MenuTanim.GetFirstOrDefault(u => u.Id == menuTanim.UstMenuId),
                };

                menuTanimVMs.Add(menuTanimVM);
            }

            return menuTanimVMs;
        }
        public List<MenuTanimVM> GetAllMenus()
        {
            List<MenuTanim> menuTanims = _unitOfWork.MenuTanim.GetByFilter(x=>x.Id>1).ToList();
            List<MenuTanimVM> menuTanimVMs = new List<MenuTanimVM>();

            foreach (var menuTanim in menuTanims)
            {
                MenuTanimVM menuTanimVM = new MenuTanimVM
                {
                    MenuTanim = menuTanim,
                    SubMenu = GetSubMenu(menuTanim.Id),
                    UstMenuTanim = _unitOfWork.MenuTanim.GetFirstOrDefault(u => u.Id == menuTanim.UstMenuId),
                };

                menuTanimVMs.Add(menuTanimVM);
            }

            return menuTanimVMs;
        }
        //public List<MenuTanimVM> GetAssignedMenuTanims(int personelId)
        //{
        //    List<PersonelMenu> personelMenus = _unitOfWork.PersonelMenu.GetByFilter(u => u.PersonelId == personelId, includeProperties:"MenuTanim");
        //    List<MenuTanimVM> menuTanimVMs = new List<MenuTanimVM>();

        //    foreach (var personelMenu in personelMenus)
        //    {

        //        MenuTanimVM menuTanimVM = new MenuTanimVM
        //        {
        //            MenuTanim = personelMenu.MenuTanim,
        //            SubMenu = GetSubMenu(personelMenu.MenuTanim.Id),
        //            UstMenuTanim = _unitOfWork.MenuTanim.GetFirstOrDefault(u => u.Id == personelMenu.MenuTanim.UstMenuId),
        //        };

        //        menuTanimVMs.Add(menuTanimVM);

        //    }

        //    return menuTanimVMs;
        //}
        public string GetMenuJson(int ustMenuId)
        {
            try
            {
                List<MenuTree> menuTreeList = new List<MenuTree>();
                menuTreeList = MenuTreeJsonGetir(menuTreeList, ustMenuId);
                string json = JsonSerializer.Serialize(menuTreeList);

                return json;

            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }
        public string GetMenuJson(string userName, int ustMenuId)
        {
            try
            {
                Personel personel = _unitOfWork.Personel.GetFirstOrDefault(p=>p.KullaniciAdi==userName) ;

                List<MenuTree> menuTreeList = new List<MenuTree>();
                menuTreeList = MenuTreeJsonGetir(personel.Id, menuTreeList, ustMenuId);
                string json = JsonSerializer.Serialize(menuTreeList);

                return json;

            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }
        private List<MenuTree> MenuTreeJsonGetir(List<MenuTree> menuTreeList, int ustMenuId)
        {
            List<MenuTanim> menuTanimList = _unitOfWork.MenuTanim.GetByFilter(u => u.UstMenuId == ustMenuId);
            var menuTanimSortedList = menuTanimList.OrderBy(x => x.Sira).ThenBy(x => x.UstMenuId);
            foreach (var item in menuTanimSortedList)
            {
                List<MenuTree> subMenuList = new List<MenuTree>();

                MenuTree menuTree = new()
                {
                    id = item.Id,
                    text = item.Adi,
                    url = item.Url,
                    webpart = item.Webpart,
                    sira = item.Sira,
                    aciklama = item.Aciklama,
                    nodes = MenuTreeJsonGetir(subMenuList, item.Id)
                };
                menuTreeList.Add(menuTree);
            }

            return menuTreeList;
        }
        private List<MenuTree> MenuTreeJsonGetir(int personelId,List<MenuTree> menuTreeList, int ustMenuId)
        {
            List<int> menuIds = _unitOfWork.PersonelMenu.GetByFilter(u => u.PersonelId == personelId).Select(m =>m.MenuTanimId).ToList();

            List<MenuTanim> menuTanimList = _unitOfWork.MenuTanim.GetByFilter(u => u.UstMenuId == ustMenuId);
            var menuTanimSortedList = menuTanimList.OrderBy(x => x.Sira).ThenBy(x => x.UstMenuId);
            //foreach (var item in menuTanimSortedList)
            //{
            //    List<MenuTree> subMenuList = new List<MenuTree>();

            //    MenuTree menuTree = new()
            //    {
            //        id = item.Id,
            //        text = item.Adi,
            //        url = item.Url,
            //        webpart = item.Webpart,
            //        sira = item.Sira,
            //        aciklama = item.Aciklama,
            //        nodes = MenuTreeJsonGetir(subMenuList, item.Id)
            //    };
            //    menuTreeList.Add(menuTree);

            //}
            foreach (var item in menuTanimSortedList)
            {
                if (menuIds.Contains(item.Id))
                {

                    List<MenuTree> subMenuList = new List<MenuTree>();
                    MenuTree menuTree = new()
                    {
                        id = item.Id,
                        text = item.Adi,
                        url = item.Url,
                        webpart = item.Webpart,
                        sira = item.Sira,
                        aciklama = item.Aciklama,
                        nodes = MenuTreeJsonGetir(personelId, subMenuList, item.Id)
                    };

                    menuTreeList.Add(menuTree);
                }
            }
            return menuTreeList;
        }
        private List<MenuTanimVM> GetSubMenu(int parentId)
        {
            List<MenuTanim> menuTanims = _unitOfWork.MenuTanim.GetByFilter(u => u.UstMenuId == parentId);
            List<MenuTanimVM> subMenu = new List<MenuTanimVM>();

            foreach (var menuTanim in menuTanims)
            {
                if (menuTanim.UstMenuId == parentId)
                {
                    MenuTanimVM menuTanimVM = new MenuTanimVM
                    {
                        MenuTanim = menuTanim,
                        SubMenu = GetSubMenu(menuTanim.Id)
                    };

                    subMenu.Add(menuTanimVM);
                }
            }

            return subMenu;
        }
        public List<MenuTanim> GetMenuListByPersonelId(int? personelId)
        {
            var personelMenus = _unitOfWork.PersonelMenu.GetPersonelMenuByPersonelId(personelId);
            var menuIds = personelMenus.Select(pt => pt.MenuTanimId);
            var menus = _unitOfWork.MenuTanim.GetMenusByIds(menuIds);

            return menus;
        }
    }

}