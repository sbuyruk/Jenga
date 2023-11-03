using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.MTS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class MTSKurumTanimController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        
        public MTSKurumTanimController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MTSKurumTanim obj)
        {
            if (ModelState.IsValid)
            {
                string? userName = HttpContext.User.Identity.Name;
                obj.Olusturan = userName;
                _unitOfWork.MTSKurumTanim.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Kurum başarıyla oluşturuldu.";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            MTSKurumTanim? mtsKurumTanimFromDb = _unitOfWork.MTSKurumTanim.GetFirstOrDefault(u => u.Id == id);
 
            if (mtsKurumTanimFromDb == null)
            {
                return NotFound();
            }
            return View(mtsKurumTanimFromDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(MTSKurumTanim obj)
        {
            if (ModelState.IsValid)
            {
                string? userName = HttpContext.User.Identity.Name;
                obj.Degistiren = userName;
                _unitOfWork.MTSKurumTanim.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Kurum başarıyla güncellendi.";
                return RedirectToAction("Index");
            }
            return View();
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<MTSKurumTanim> objMTSKurumTanimListesi = _unitOfWork.MTSKurumTanim.GetAll().ToList();
            return Json(new { data = objMTSKurumTanimListesi });
        }

        [HttpPost]
        public IActionResult Delete(int? id)
        {
            var mtsKurumTanimToBeDeleted = _unitOfWork.MTSKurumTanim.GetFirstOrDefault(u => u.Id == id);
            var kurumlaIliskiliKisi = _unitOfWork.MTSKurumGorev.GetFirstOrDefault(u => u.MTSKurumTanimId == id);
            
            if (mtsKurumTanimToBeDeleted == null)
            {
                return Json(new { success = false, message = "Kurum Bulunamadı." });
            }
            else
            if(kurumlaIliskiliKisi != null)
            {
                return Json(new { success = false, message = "Bu Kurum ile bağlantılı ziyaretçi kayıt vardır." });
            }
            else 
            {
                _unitOfWork.MTSKurumTanim.Remove(mtsKurumTanimToBeDeleted);
                _unitOfWork.Save();

                return Json(new { success = true, message = "Kurum başarıyla silindi." }); 
            }

        }

        #endregion

    }
}
