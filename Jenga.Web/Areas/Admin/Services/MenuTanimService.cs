using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.Ortak;

namespace Jenga.Web.Areas.Admin.Services
{
    public class MenuTanimService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MenuTanimService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<MenuTanimVM> GetAllMenuTanims(int parentId)
        {
        List<MenuTanim> menuTanims = _unitOfWork.MenuTanim.GetByFilter(u => u.UstMenuId == parentId);
        List<MenuTanimVM> menuTanimVMs = new List<MenuTanimVM>();

            foreach (var menuTanim in menuTanims)
            {
                //if (menuTanim.ParentId == null)
                {
                    MenuTanimVM menuTanimVM = new MenuTanimVM
                    {
                        MenuTanim = menuTanim,
                        SubMenu = GetSubMenu(menuTanim.Id)
                    };

                    menuTanimVMs.Add(menuTanimVM);
                }
            }

            return menuTanimVMs;
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
    }

}