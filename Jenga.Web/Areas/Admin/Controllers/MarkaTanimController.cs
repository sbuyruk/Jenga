using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.DYS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class MarkaTanimController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public MarkaTanimController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
            _cache = cache;
        }
        private readonly IMemoryCache _cache;
        public IActionResult Index()
        {
            IEnumerable<MarkaTanim> obj = _unitOfWork.MarkaTanim.GetAll();
            return View(obj);
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
            var markaTanimFromDb = _unitOfWork.MarkaTanim.GetFirstOrDefault(u => u.Id == id);
            if (markaTanimFromDb == null)
            {
                return NotFound();
            }
            return View(markaTanimFromDb);

        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var markaList = _unitOfWork.MarkaTanim.GetAll();
            return Json(new { data = markaList });
            //return Json(new { data = cachedObject });
        }
        private IEnumerable<MarkaTanim> GetDataFromDataSource()
        {
            // Code to fetch the data from the data source
            // ...
            var markaList = _unitOfWork.MarkaTanim.GetAll();
            return markaList;
        }
        //Delete
        [HttpPost]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _unitOfWork.MarkaTanim.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Kayıt silmede hata" });
            }
            _unitOfWork.MarkaTanim.Remove(obj);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Marka silindi" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MarkaTanim obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.Id == 0)
                {
                    string? userName = HttpContext.User.Identity.Name;
                    obj.Olusturan = userName;
                    _unitOfWork.MarkaTanim.Add(obj);
                    TempData["success"] = "Marka Tanımı oluşturuldu";
                }
                else
                {
                    TempData["error"] = "Marka Tanımı oluşturulamadı";
                }
                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(MarkaTanim obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.Id == 0)
                {
                    TempData["success"] = "Marka Tanım Id bulunamadı";
                }
                else
                {
                    string? userName = HttpContext.User.Identity.Name;
                    obj.Degistiren = userName;
                    _unitOfWork.MarkaTanim.Update(obj);
                    TempData["success"] = "Maarka Tanımı güncellendi";
                }
                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }
        #endregion
    }


}
