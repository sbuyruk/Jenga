using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.MTS;
using Jenga.Models.Ortak;
using Microsoft.AspNetCore.Mvc;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class ModulTanimController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ModulTanimController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        
        public IActionResult Index()
        {
            IEnumerable<ModulTanim> objModulTanimList = _unitOfWork.ModulTanim.GetAll();
            return View(objModulTanimList);
        }
        //POST
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ModulTanim obj)
        {
            if (string.IsNullOrEmpty(obj.Adi.Trim()))
            {
                ModelState.AddModelError("adi", "Lütfen Modül Adını boş bırakmayınız");
            }
            if (ModelState.IsValid)
            {
                string? userName = HttpContext.User.Identity.Name;
                obj.Olusturan = userName;
                _unitOfWork.ModulTanim.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Modül kayıtlara eklendi.";
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
            var obj = _unitOfWork.ModulTanim.GetFirstOrDefault(u => u.Id == id);
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
        public IActionResult Edit(ModulTanim obj)
        {
            if (string.IsNullOrEmpty(obj.Adi.Trim()))
            {
                ModelState.AddModelError("adi", "Lütfen Modül Adını boş bırakmayınız");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.ModulTanim.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Modül kaydı güncellendi";
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
            var depoTanimFromDb = _unitOfWork.ModulTanim.GetFirstOrDefault(u => u.Id == id);
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
            var obj = _unitOfWork.ModulTanim.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            else
            {
                _unitOfWork.ModulTanim.Remove(obj);
                _unitOfWork.Save();
                TempData["success"] = "Modul Tanımı silindi";
                return RedirectToAction("Index");
            }
        }
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var modulTanimList = _unitOfWork.ModulTanim.GetAll();
            return Json(new { data = modulTanimList });
        }
        #endregion
    }
}
