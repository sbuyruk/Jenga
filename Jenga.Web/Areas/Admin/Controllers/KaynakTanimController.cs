using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.MTS;
using Microsoft.AspNetCore.Mvc;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class KaynakTanimController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public KaynakTanimController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            IEnumerable<KaynakTanim> objKaynakTanimList = _unitOfWork.KaynakTanim.GetAll();
            return View(objKaynakTanimList);
        }
        //POST
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(KaynakTanim obj)
        {
            if (string.IsNullOrEmpty(obj.Adi.Trim()))
            {
                ModelState.AddModelError("adi", "Lütfen Kaynak Adını boş bırakmayınız");
            }
            if (ModelState.IsValid)
            {
                string? userName = HttpContext.User.Identity.Name;
                obj.Olusturan = userName;
                _unitOfWork.KaynakTanim.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Kaynak kayıtlara eklendi.";
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
            var kaynakTanimFromDb = _unitOfWork.KaynakTanim.GetFirstOrDefault(u => u.Id == id);
            if (kaynakTanimFromDb == null)
            {
                return NotFound();
            }
            return View(kaynakTanimFromDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(KaynakTanim obj)
        {
            if (string.IsNullOrEmpty(obj.Adi.Trim()))
            {
                ModelState.AddModelError("adi", "Lütfen Kaynak Adını boş bırakmayınız");
            }
            if (ModelState.IsValid)
            {
                string? userName = HttpContext.User.Identity.Name;
                obj.Degistiren = userName;
                _unitOfWork.KaynakTanim.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Kaynak kaydı güncellendi";
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
            var kaynakTanimFromDb = _unitOfWork.KaynakTanim.GetFirstOrDefault(u => u.Id == id);
            if (kaynakTanimFromDb == null)
            {
                return NotFound();
            }
            return View(kaynakTanimFromDb);
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
            var obj = _unitOfWork.KaynakTanim.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            else
            {
                _unitOfWork.KaynakTanim.Remove(obj);
                _unitOfWork.Save();
                TempData["success"] = "Kaynak tanımı silindi";
                return RedirectToAction("Index");
            }
        }
    }
}
