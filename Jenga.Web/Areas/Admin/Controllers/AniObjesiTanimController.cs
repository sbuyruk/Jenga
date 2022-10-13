using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models;
using Jenga.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class AniObjesiTanimController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public AniObjesiTanimController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }
        
        //GET
        public IActionResult Upsert(int? id)
        {
            var stokDurumuList = new List<SelectListItem> {
              new SelectListItem { Text = "Stoklu", Value = "Stoklu" },
              new SelectListItem { Text = "Stoksuz", Value = "Stoksuz" }
            };
            AniObjesiVM aniObjesiVM = new()
            {
                AniObjesiTanim = new(),
                KaynakTanimList = _unitOfWork.KaynakTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
                StokDurumuList = stokDurumuList

            };

            if (id == null || id == 0)
            {
                //create 
                return View(aniObjesiVM);
            }
            else
            {
                //update 
                aniObjesiVM.AniObjesiTanim = _unitOfWork.AniObjesiTanim.GetFirstOrDefault(u => u.Id == id);
                return View(aniObjesiVM);
            }
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(AniObjesiVM obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.AniObjesiTanim.Id == 0)
                {
                    _unitOfWork.AniObjesiTanim.Add(obj.AniObjesiTanim);
                    TempData["success"] = "Anı Objesi oluşturuldu";
                }
                else
                {
                    _unitOfWork.AniObjesiTanim.Update(obj.AniObjesiTanim);
                    TempData["success"] = "Anı Objesi güncellendi";
                }
                _unitOfWork.Save();
                
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var aniObjesiList = _unitOfWork.AniObjesiTanim.GetAll(includeProperties:"KaynakTanim");
            return Json(new { data = aniObjesiList });
        }

        //Delete
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _unitOfWork.AniObjesiTanim.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new {success=false, message="Kayıt silmede hata"});
            }
            _unitOfWork.AniObjesiTanim.Remove(obj);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Anı Objesi silindi" });
        }
        #endregion
    }


}
