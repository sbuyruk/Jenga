using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.MTS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting;
using Jenga.Models.Ortak;
using Jenga.Models.IKYS;
using Jenga.DataAccess.Repository;
using Jenga.Web.Areas.Admin.Services;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class PersonelMenuController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly MenuTanimService _menuTanimService;
        public PersonelMenuController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment, MenuTanimService menuTanimService)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
            _menuTanimService = menuTanimService;
        }
        public IActionResult Index()
        {
            return View();
        }
        
        //GET
        public IActionResult Create(int? id)
        {

            PersonelMenuVM personelMenuVM = new()
            {

                PersonelMenu = new(),
                MenuTanimSelecList = _unitOfWork.MenuTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
                Personel = _unitOfWork.Personel.GetFirstOrDefault(u => u.Id == id)
            };

            return View(personelMenuVM);


        }
        public IActionResult Edit(int? id)
        {
            Personel personel = _unitOfWork.Personel.GetFirstOrDefault(u => u.Id == id);
            List<MenuTanim> selectesMenuList = GetMenuListByPersonelId(personel.Id);
            List<MenuTanimVM> menuTanimList = _menuTanimService.GetAllMenuTanims(1);
            PersonelMenuVM personelMenuVM = new()
            {
                PersonelMenu = new(),
                MenuTanimSelecList = _unitOfWork.MenuTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
                Personel = _unitOfWork.Personel.GetFirstOrDefault(u => u.Id == id),
                MenuTanimList = menuTanimList,
                SelectedMenuTanimList = selectesMenuList
            };

            if (id == null || id == 0)
            {
                return NotFound();

            }
            else
            {
                //update 
                personelMenuVM.PersonelMenu = _unitOfWork.PersonelMenu.GetFirstOrDefault(u => u.PersonelId == id);
                return View(personelMenuVM);
            }

        }

        public List<MenuTanim> GetMenuListByPersonelId(int? personelId)
        {
            var personelMenus = _unitOfWork.PersonelMenu.GetPersonelMenuByPersonelId(personelId);
            var menuIds = personelMenus.Select(pt => pt.MenuTanimId);
            var menus = _unitOfWork.MenuTanim.GetMenusByIds(menuIds);

            return menus;
        }
        public List<MenuTanimVM> GetMenuListByParentId(int parentId)
        {
            List<MenuTanimVM> menuList= _unitOfWork.MenuTanim.GetSubMenuMenuListByParentId(parentId);
            //foreach (var menu in menus)
            //{
            //    var submenu = _unitOfWork.MenuTanim.GetSubMenuMenuListByParentId(menu.MenuTanim.Id);
            //    MenuTanimVM menuTanimVM = new()
            //    {
            //        MenuTanim = menu.MenuTanim,
            //        SubMenu = submenu,

            //    };
            //    menuList.Add(menuTanimVM);
            //    if (submenu != null)
            //    {
                   
            //        GetMenuListByParentId(menu.MenuTanim.Id);
            //    }
               
                
            //}
        
            return menuList;
        }
        public IEnumerable<MenuTanim> GetMenuTanimAll()
        {
            var menus = _unitOfWork.MenuTanim.GetAll();

            return menus;
        }
        public string GetMenuAll()
        {
            int rootMenuId = 1;
            string json = _unitOfWork.MenuTanim.GetMenuJson(rootMenuId);

            return json;
        }
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            //var personelList = _unitOfWork.Personel.GetAll();
            var personelMenuList = _unitOfWork.PersonelMenu.GetAll(includeProperties: "Personel,MenuTanim");
            return Json(new { data = personelMenuList });
        }
        public IActionResult GetPersonelAll()
        {
            var personelMenus = _unitOfWork.PersonelMenu.GetAll(includeProperties: "Personel,MenuTanim");
            var personel = _unitOfWork.Personel.GetAll();

            var personelMenuList = personel
                .GroupJoin(personelMenus,
                    p => p.Id,
                    pm => pm.PersonelId,
                    (p, pm) => new {
                        Personel = p,
                        MenuTanim = string.Join(", ", pm.Select(t => t.MenuTanim.Adi))
                    });

            return Json(new { data = personelMenuList });
        }
       
        //Delete
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _unitOfWork.DepoHareket.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new {success=false, message="Kayıt silmede hata"});
            }

            _unitOfWork.DepoHareket.Remove(obj);
            DepoHareketVM personelMenuVM = new()
            {
                DepoHareket = obj
               
            };
            _unitOfWork.Save();
            return Json(new { success = true, message = "Yetki silindi" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(DepoHareketVM obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.DepoHareket.Id == 0)
                {
                    _unitOfWork.DepoHareket.Add(obj.DepoHareket);
                    TempData["success"] = "Menu işlemi gerçekleşti";
                }
                else
                {
                    TempData["error"] = "PersonelMenu Id bulunamadı";
                }

                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(DepoHareketVM obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.DepoHareket.Id == 0)
                {
                    TempData["error"] = "PersonelMenu Id bulunamadı";
                }
                else
                {
                    _unitOfWork.DepoHareket.Update(obj.DepoHareket);
                    TempData["success"] = "Yetki işlemi güncellendi";
                }

                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }
        #endregion

    }


}