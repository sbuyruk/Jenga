using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Ortak;
using Microsoft.AspNetCore.Mvc;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class IlController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public IlController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            IEnumerable<Il> objIlList = _unitOfWork.Il.GetAll();
            return View(objIlList);
        }
        //POST
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Il obj)
        {
            if (string.IsNullOrEmpty(obj.IlAdi.Trim()))
            {
                ModelState.AddModelError("adi", "Lütfen Kaynak Adını boş bırakmayınız");
            }
            if (ModelState.IsValid)
            {
                string? userName = HttpContext.User.Identity.Name;
                obj.Olusturan = userName;
                _unitOfWork.Il.Add(obj);
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
            var ilFromDb = _unitOfWork.Il.GetFirstOrDefault(u => u.Id == id);
            if (ilFromDb == null)
            {
                return NotFound();
            }
            return View(ilFromDb);
        }
        #region API Calls
        public JsonResult GetIlceListByIlId(int ilId)
        {
            List<Ilce> ilceList = _unitOfWork.Ilce.GetByFilter(u => u.IlId == ilId).ToList();
            return Json(ilceList);
        }
        #endregion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Il obj)
        {
            if (string.IsNullOrEmpty(obj.IlAdi.Trim()))
            {
                ModelState.AddModelError("adi", "Lütfen Kaynak Adını boş bırakmayınız");
            }
            if (ModelState.IsValid)
            {
                string? userName = HttpContext.User.Identity.Name;
                obj.Degistiren = userName;
                _unitOfWork.Il.Update(obj);
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
            var ilFromDb = _unitOfWork.Il.GetFirstOrDefault(u => u.Id == id);
            if (ilFromDb == null)
            {
                return NotFound();
            }
            return View(ilFromDb);
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
            var obj = _unitOfWork.Il.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            else
            {
                _unitOfWork.Il.Remove(obj);
                _unitOfWork.Save();
                TempData["success"] = "Kaynak tanımı silindi";
                return RedirectToAction("Index");
            }
        }
    }
}
