using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.IKYS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class PersonelController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public PersonelController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
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
                Personel personel = _unitOfWork.Personel.GetFirstOrDefault(u => u.Id == id);
                return View(personel);
            }

        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var personelList = _unitOfWork.Personel.GetAll(includeProperties: "IsBilgileri,GorevTanim");
            return Json(new { data = personelList });
        }

        //Delete
        [HttpPost]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _unitOfWork.Personel.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Kayıt silmede hata" });
            }
            _unitOfWork.Personel.Remove(obj);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Personel silindi" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Personel obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.Id == 0)
                {
                    _unitOfWork.Personel.Add(obj);
                    TempData["success"] = "Personel oluşturuldu";
                }
                else
                {
                    TempData["error"] = "Personel oluşturulamadı";
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
        public IActionResult Edit(Personel obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.Id == 0)
                {
                    TempData["success"] = "Personel Id bulunamadı";
                }
                else
                {
                    _unitOfWork.Personel.Update(obj);
                    TempData["success"] = "Personel güncellendi";
                }
                string? userName = HttpContext.User.Identity.Name;
                obj.Degistiren = userName;
                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }
        #endregion
    }
}
