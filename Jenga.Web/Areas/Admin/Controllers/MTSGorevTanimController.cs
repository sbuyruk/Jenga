using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.MTS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class MTSGorevTanimController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public MTSGorevTanimController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var objMTSGorevTanimListesi = _unitOfWork.MTSGorevTanim.GetAll();
            return View(objMTSGorevTanimListesi);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MTSGorevTanim obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.Id == 0)
                {
                    _unitOfWork.MTSGorevTanim.Add(obj);
                    TempData["success"] = "Görev oluşturuldu";
                }
                else
                {
                    TempData["error"] = "Görev oluşturulamadı";
                }

                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }

        public IActionResult Edit(int? id)
        {

            if (id == null || id == 0)
            {
                return NotFound();
            }
            else
            {
                //update 
                MTSGorevTanim? mTSGorevTanim = _unitOfWork.MTSGorevTanim.GetFirstOrDefault(u => u.Id == id);
                return View(mTSGorevTanim);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(MTSGorevTanim obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.Id == 0)
                {
                    TempData["success"] = "Görev bulunamadı";
                }
                else
                {
                    _unitOfWork.MTSGorevTanim.Update(obj);
                    TempData["success"] = "Görev güncellendi";
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
            List<MTSGorevTanim> objMTSGorevTanimListesi = _unitOfWork.MTSGorevTanim.GetAll().ToList();
            return Json(new { data = objMTSGorevTanimListesi });
        }

        [HttpPost]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _unitOfWork.MTSGorevTanim.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Görev silinemedi" });
            }
            _unitOfWork.MTSGorevTanim.Remove(obj);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Görev silindi" });

        }

        #endregion

    }
}
