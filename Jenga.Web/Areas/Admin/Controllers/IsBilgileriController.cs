using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.IKYS;
using Microsoft.AspNetCore.Mvc;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class IsBilgileriController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public IsBilgileriController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }

        //GET
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
            else
            {
                //update 
                IsBilgileri isBilgileri = _unitOfWork.IsBilgileri.GetFirstOrDefault(u => u.Id == id);
                return View(isBilgileri);
            }

        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var personelIsBilgileriList = _unitOfWork.IsBilgileri.GetAll(includeProperties: "Personel,BirimTanim,GorevTanim,UnvanTanim");
            return Json(new { data = personelIsBilgileriList });
        }

        //Delete
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _unitOfWork.IsBilgileri.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Kayıt silmede hata" });
            }
            _unitOfWork.IsBilgileri.Remove(obj);
            _unitOfWork.Save();
            return Json(new { success = true, message = "IsBilgileri silindi" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(IsBilgileri obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.Id == 0)
                {
                    _unitOfWork.IsBilgileri.Add(obj);
                    TempData["success"] = "IsBilgileri oluşturuldu";
                }
                else
                {
                    TempData["error"] = "IsBilgileri oluşturulamadı";
                }
                string? userName = HttpContext.User.Identity.Name;
                obj.Olusturan = userName;
                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(IsBilgileri obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.Id == 0)
                {
                    TempData["success"] = "IsBilgileri Id bulunamadı";
                }
                else
                {
                    _unitOfWork.IsBilgileri.Update(obj);
                    TempData["success"] = "IsBilgileri güncellendi";
                }

                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }
        #endregion
    }
}
