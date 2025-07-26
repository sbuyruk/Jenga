using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.MTS;
using Microsoft.AspNetCore.Mvc;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class FaaliyetKatilimController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public FaaliyetKatilimController(IUnitOfWork unitOfWork)
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
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            FaaliyetKatilim? faaliyetYeriFromDb = _unitOfWork.FaaliyetKatilim.GetFirstOrDefault(u => u.Id == id);

            if (faaliyetYeriFromDb == null)
            {
                return NotFound();
            }
            return View(faaliyetYeriFromDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(FaaliyetKatilim obj)
        {
            if (ModelState.IsValid)
            {
                string? userName = HttpContext.User.Identity.Name;
                obj.Olusturan = userName;
                _unitOfWork.FaaliyetKatilim.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Faaliyet Yeri  başarıyla oluşturuldu.";
                return RedirectToAction("Index");
            }
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(FaaliyetKatilim obj)
        {
            if (ModelState.IsValid)
            {
                string? userName = HttpContext.User.Identity.Name;
                obj.Degistiren = userName;
                _unitOfWork.FaaliyetKatilim.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Faaliyet Yeri başarıyla güncellendi.";
                return RedirectToAction("Index");
            }
            return View();
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<FaaliyetKatilim> list = _unitOfWork.FaaliyetKatilim.GetAll().ToList();
            return Json(new { data = list });
        }

        [HttpPost]
        public IActionResult Delete(int? id)
        {
            var faaliyetYeriToBeDeleted = _unitOfWork.FaaliyetKatilim.GetFirstOrDefault(u => u.Id == id);
            //var faaliyetYeriKullanimi = _unitOfWork.Faaliyet.GetFirstOrDefault(u => u.FaaliyetYeriId == id);

            if (faaliyetYeriToBeDeleted == null)
            {
                return Json(new { success = false, message = "Faaliyet Yeri  Bulunamadı." });
            }
            else
            //if(faaliyetYeriKullanimi != null)
            //{
            //    return Json(new { success = false, message = "Bu Faaliyet Yeri ile bağlantılı ziyaretçi kayıt vardır." });
            //}
            //else 
            {
                _unitOfWork.FaaliyetKatilim.Remove(faaliyetYeriToBeDeleted);
                _unitOfWork.Save();

                return Json(new { success = true, message = "Faaliyet Yeri  başarıyla silindi." });
            }

        }

        #endregion

    }
}
