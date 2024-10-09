using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.DYS;
using Jenga.Models.MTS;
using Microsoft.AspNetCore.Mvc;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class DepoTanimController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public DepoTanimController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        
        public IActionResult Index()
        {
            IEnumerable<DepoTanim> objDepoTanimList = _unitOfWork.DepoTanim.GetAll();
            return View(objDepoTanimList);
        }
        //POST
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(DepoTanim obj)
        {
            if (string.IsNullOrEmpty(obj.Adi.Trim()))
            {
                ModelState.AddModelError("adi", "Lütfen Depo Adını boş bırakmayınız");
            }
            if (ModelState.IsValid)
            {
                string? userName = HttpContext.User.Identity.Name;
                obj.Olusturan = userName;
                _unitOfWork.DepoTanim.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Depo kayıtlara eklendi.";
                return RedirectToAction("Index");
            }
            return View(obj);
        }
        //GET
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _unitOfWork.DepoTanim.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            string? userName = HttpContext.User.Identity.Name;
            obj.Olusturan = userName;
            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(DepoTanim obj)
        {
            if (string.IsNullOrEmpty(obj.Adi.Trim()))
            {
                ModelState.AddModelError("adi", "Lütfen Depo Adını boş bırakmayınız");
            }
            if (ModelState.IsValid)
            {
                string? userName = HttpContext.User.Identity.Name;
                obj.Degistiren = userName;
                _unitOfWork.DepoTanim.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Depo kaydı güncellendi";
                return RedirectToAction("Index");
            }
            return View(obj);
        }
        //GET
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var depoTanimFromDb = _unitOfWork.DepoTanim.GetFirstOrDefault(u => u.Id == id);
            if (depoTanimFromDb == null)
            {
                return NotFound();
            }
            return View(depoTanimFromDb);
        }
        //POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _unitOfWork.DepoTanim.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            else
            {
                _unitOfWork.DepoTanim.Remove(obj);
                _unitOfWork.Save();
                TempData["success"] = "Depo tanımı silindi";
                return RedirectToAction("Index");
            }
        }
    }
}
