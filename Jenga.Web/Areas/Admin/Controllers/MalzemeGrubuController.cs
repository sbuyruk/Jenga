using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.DYS;
using Microsoft.AspNetCore.Mvc;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class MalzemeGrubuController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public MalzemeGrubuController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            IEnumerable<MalzemeGrubu> objMalzemeGrubuList = _unitOfWork.MalzemeGrubu.GetAll();
            return View(objMalzemeGrubuList);
        }
        //POST
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MalzemeGrubu obj)
        {
            if (string.IsNullOrEmpty(obj.Adi.Trim()))
            {
                ModelState.AddModelError("adi", "Lütfen Malzeme Grubu Tanımını boş bırakmayınız");
            }
            if (ModelState.IsValid)
            {
                string? userName = HttpContext.User.Identity.Name;
                obj.Olusturan = userName;
                _unitOfWork.MalzemeGrubu.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Malzeme Grubu tanımı kayıtlara eklendi.";
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
            var malzemeGrubuFromDb = _unitOfWork.MalzemeGrubu.GetFirstOrDefault(u => u.Id == id);
            if (malzemeGrubuFromDb == null)
            {
                return NotFound();
            }
            return View(malzemeGrubuFromDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(MalzemeGrubu obj)
        {
            if (string.IsNullOrEmpty(obj.Adi.Trim()))
            {
                ModelState.AddModelError("adi", "Lütfen Malzeme Grubu tanımını boş bırakmayınız");
            }
            if (ModelState.IsValid)
            {
                string? userName = HttpContext.User.Identity.Name;
                obj.Degistiren = userName;
                _unitOfWork.MalzemeGrubu.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Malzeme Grubu tanım kaydı güncellendi";
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
            var malzemeGrubuFromDb = _unitOfWork.MalzemeGrubu.GetFirstOrDefault(u => u.Id == id);
            if (malzemeGrubuFromDb == null)
            {
                return NotFound();
            }
            return View(malzemeGrubuFromDb);
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
            var obj = _unitOfWork.MalzemeGrubu.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            else
            {
                _unitOfWork.MalzemeGrubu.Remove(obj);
                _unitOfWork.Save();
                TempData["success"] = "Malzeme Grubu tanımı silindi";
                return RedirectToAction("Index");
            }
        }
    }
}
