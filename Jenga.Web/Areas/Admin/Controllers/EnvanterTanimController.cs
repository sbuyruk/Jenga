using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.DYS;
using Microsoft.AspNetCore.Mvc;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class EnvanterTanimController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public EnvanterTanimController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            IEnumerable<EnvanterTanim> objEnvanterTanimList = _unitOfWork.EnvanterTanim.GetAll();
            return View(objEnvanterTanimList);
        }
        //Get
        public IActionResult Create()
        {
            return View();
        }
        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(EnvanterTanim obj)
        {
            if (string.IsNullOrEmpty(obj.Adi.Trim()))
            {
                ModelState.AddModelError("adi", "Lütfen Envanter Tanımını boş bırakmayınız");
            }
            if (ModelState.IsValid)
            {
                string? userName = HttpContext.User.Identity.Name;
                obj.Olusturan = userName;
                _unitOfWork.EnvanterTanim.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Envanter tanımı kayıtlara eklendi.";
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
            var envanterTanimFromDb = _unitOfWork.EnvanterTanim.GetFirstOrDefault(u => u.Id == id);
            if (envanterTanimFromDb == null)
            {
                return NotFound();
            }
            return View(envanterTanimFromDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EnvanterTanim obj)
        {
            if (string.IsNullOrEmpty(obj.Adi.Trim()))
            {
                ModelState.AddModelError("adi", "Lütfen Envanter tanımını boş bırakmayınız");
            }
            if (ModelState.IsValid)
            {
                string? userName = HttpContext.User.Identity.Name;
                obj.Degistiren = userName;
                _unitOfWork.EnvanterTanim.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Envanter tanım kaydı güncellendi";
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
            var envanterTanimFromDb = _unitOfWork.EnvanterTanim.GetFirstOrDefault(u => u.Id == id);
            if (envanterTanimFromDb == null)
            {
                return NotFound();
            }
            return View(envanterTanimFromDb);
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
            var obj = _unitOfWork.EnvanterTanim.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            else
            {
                _unitOfWork.EnvanterTanim.Remove(obj);
                _unitOfWork.Save();
                TempData["success"] = "Envanter tanımı silindi";
                return RedirectToAction("Index");
            }
        }
    }
}
