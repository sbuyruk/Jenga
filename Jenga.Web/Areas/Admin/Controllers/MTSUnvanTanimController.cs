using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.MTS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class MTSUnvanTanimController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        
        public MTSUnvanTanimController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<MTSUnvanTanim> objMTSUnvanTanimListesi = _unitOfWork.MTSUnvanTanim.GetAll().ToList();
            return View(objMTSUnvanTanimListesi);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MTSUnvanTanim obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.MTSUnvanTanim.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Ünvan başarıyla oluşturuldu.";
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
            MTSUnvanTanim? mtsUnvanTanimFromDb = _unitOfWork.MTSUnvanTanim.GetFirstOrDefault(u => u.Id == id);
 
            if (mtsUnvanTanimFromDb == null)
            {
                return NotFound();
            }
            return View(mtsUnvanTanimFromDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(MTSUnvanTanim obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.MTSUnvanTanim.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Ünvan başarıyla güncellendi.";
                return RedirectToAction("Index");
            }
            return View();
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<MTSUnvanTanim> objMTSUnvanTanimListesi = _unitOfWork.MTSUnvanTanim.GetAll().ToList();
            return Json(new { data = objMTSUnvanTanimListesi });
        }

        [HttpPost]
        public IActionResult Delete(int? id)
        {
            var mtsUnvanTanimToBeDeleted = _unitOfWork.MTSUnvanTanim.GetFirstOrDefault(u => u.Id == id);
            
            
            if (mtsUnvanTanimToBeDeleted == null)
            {
                return Json(new { success = false, message = "Ünvan silinemedi." });
            }
            else 
            {
                _unitOfWork.MTSUnvanTanim.Remove(mtsUnvanTanimToBeDeleted);
                _unitOfWork.Save();

                return Json(new { success = true, message = "Ünvan başarıyla silindi." }); 
            }

        }

        #endregion

    }
}
