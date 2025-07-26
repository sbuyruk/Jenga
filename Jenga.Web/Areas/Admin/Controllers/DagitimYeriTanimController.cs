using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.MTS;
using Microsoft.AspNetCore.Mvc;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class DagitimYeriTanimController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public DagitimYeriTanimController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<DagitimYeriTanim> objDagitimYeriTanimList = _unitOfWork.DagitimYeriTanim.GetAll();
            return View(objDagitimYeriTanimList);
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
            var obj = _unitOfWork.DagitimYeriTanim.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            string? userName = HttpContext.User.Identity.Name;
            obj.Olusturan = userName;
            return View(obj);
        }
        //POST

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(DagitimYeriTanim obj)
        {
            if (string.IsNullOrEmpty(obj.Adi.Trim()))
            {
                ModelState.AddModelError("adi", "Lütfen Depo Adını boş bırakmayınız");
            }
            if (ModelState.IsValid)
            {
                string? userName = HttpContext.User.Identity.Name;
                obj.Olusturan = userName;
                _unitOfWork.DagitimYeriTanim.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Depo kayıtlara eklendi.";
                return RedirectToAction("Index");
            }
            return View(obj);
        }
        //GET

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(DagitimYeriTanim obj)
        {
            if (string.IsNullOrEmpty(obj.Adi.Trim()))
            {
                ModelState.AddModelError("adi", "Lütfen Depo Adını boş bırakmayınız");
            }
            if (ModelState.IsValid)
            {
                string? userName = HttpContext.User.Identity.Name;
                obj.Olusturan = userName;
                _unitOfWork.DagitimYeriTanim.Update(obj);
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
            var dagitimYeriTanimFromDb = _unitOfWork.DagitimYeriTanim.GetFirstOrDefault(u => u.Id == id);
            if (dagitimYeriTanimFromDb == null)
            {
                return NotFound();
            }
            return View(dagitimYeriTanimFromDb);
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
            var obj = _unitOfWork.DagitimYeriTanim.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            else
            {
                _unitOfWork.DagitimYeriTanim.Remove(obj);
                _unitOfWork.Save();
                TempData["success"] = "Dağıtım yeri silindi";
                return RedirectToAction("Index");
            }
        }
    }
}
