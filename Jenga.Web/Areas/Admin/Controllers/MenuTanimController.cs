using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.IKYS;
using Jenga.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class MenuTanimController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public MenuTanimController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
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
                UstMenuTanimList = _unitOfWork.MenuTanim.GetAll().Select(i => new SelectListItem
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
                UstMenuTanimList = _unitOfWork.MenuTanim.GetAll().Select(i => new SelectListItem
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
                return View(menuTanimVM);
            }
        }
       
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var menuTanimList = _unitOfWork.MenuTanim.GetAll();

            return Json(new { data = menuTanimList });
        }
        public string GetMenuAll()
        {
            int rootMenuId = 0;
            string json = _unitOfWork.MenuTanim.GetMenuAll(rootMenuId);

            return json;
        }
        //Delete
        [HttpDelete]
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
