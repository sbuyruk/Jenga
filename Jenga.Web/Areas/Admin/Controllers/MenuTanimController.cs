using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.Ortak;
using Jenga.Web.Areas.Admin.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class MenuTanimController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly MenuService _menuService;
        public MenuTanimController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment, MenuService menuService)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
            _menuService = menuService;
        }
        public IActionResult Index()
        {
            
            return View();
        }
        
        //GET
        public IActionResult Create()
        {
            MenuTanimVM menuTanimVM = new()
            {
                MenuTanim = new(),
                UstMenuSelectList = _unitOfWork.MenuTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
            };
            
            return View(menuTanimVM);
   
            
        }
        public IActionResult Edit(int? id)
        {
            MenuTanimVM menuTanimVM = new()
            {
                MenuTanim = new(),
                UstMenuSelectList = _unitOfWork.MenuTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
                
            };

            if (id == null || id == 0)
            {
                return NotFound();
            }
            else
            {
                //update 
                menuTanimVM.MenuTanim = _unitOfWork.MenuTanim.GetFirstOrDefault(u => u.Id == id);
                menuTanimVM.UstMenuTanim = _unitOfWork.MenuTanim.GetFirstOrDefault(u => u.UstMenuId == id);
                return View(menuTanimVM);
            }
        }
       
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var menuTanimList = _unitOfWork.MenuTanim.GetAll();
            var menuTanimSortedList = menuTanimList.OrderBy(x => x.Sira).ThenBy(x => x.UstMenuId);

            return Json(new { data = menuTanimSortedList });
        }
        public IActionResult GetMenuTanimList()
        {
            var menuTanimList = _menuService.GetAllMenus();
            var menuTanimSortedList = menuTanimList.OrderBy(x => x.MenuTanim.Sira).ThenBy(x => x.MenuTanim.UstMenuId);

            return Json(new { data = menuTanimSortedList });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>Tüm MenuTanim nesneleri json formatında ve recursive</returns>
        public string GetMenuAll()
        {
            int rootMenuId = 1;
            string json = _menuService.GetMenuJson(rootMenuId);
            return json;
        }
        public string GetMenuByPersonId()
        {
            int rootMenuId = 1;
            string? userName = "asbuyruk";//HttpContext.User?.Identity?.Name?.Split('\\')[1];//
            string json = _menuService.GetMenuJson(userName, rootMenuId);
            return json;
        }
        //Delete
        [HttpPost]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _unitOfWork.MenuTanim.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new {success=false, message="Kayıt silmede hata"});
            }

            _unitOfWork.MenuTanim.Remove(obj);
            MenuTanimVM menuTanimVM = new()
            {
                MenuTanim = obj
               
            };
            _unitOfWork.Save();
            return Json(new { success = true, message = "Menü kaydı silindi" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MenuTanimVM obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.MenuTanim.Id == 0)
                {
                    string? userName = HttpContext.User.Identity.Name;
                    obj.MenuTanim.Olusturan = userName;
                    _unitOfWork.MenuTanim.Add(obj.MenuTanim);
                    TempData["success"] = "Menü oluşturuldu";
                }
                else
                {
                    TempData["error"] = "Menü Id bulunamadı";
                }
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return View(obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(MenuTanimVM obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.MenuTanim.Id == 0)
                {
                    TempData["error"] = "Menü Id bulunamadı";
                }
                else
                {
                    string? userName = HttpContext.User.Identity.Name;
                    obj.MenuTanim.Degistiren = userName;
                    _unitOfWork.MenuTanim.Update(obj.MenuTanim);
                    TempData["success"] = "Menü kaydı güncellendi";
                }

                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }
        #endregion
    }


}
