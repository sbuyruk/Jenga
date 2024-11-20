using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.DYS;
using Jenga.Models.MTS;
using Jenga.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class MalzemeYeriTanimController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public MalzemeYeriTanimController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
            _cache = cache;
        }
        private readonly IMemoryCache _cache;
        public IActionResult Index()
        {
            IEnumerable<MalzemeYeriTanim> obj = _unitOfWork.MalzemeYeriTanim.GetAll();
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
            var malzemeYeriTanimFromDb = _unitOfWork.MalzemeYeriTanim.GetFirstOrDefault(u => u.Id == id);
            if (malzemeYeriTanimFromDb == null)
            {
                return NotFound();
            }
            return View(malzemeYeriTanimFromDb);

        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var malzemeYeriList = _unitOfWork.MalzemeYeriTanim.GetAll();
            return Json(new { data = malzemeYeriList });
            //return Json(new { data = cachedObject });
        }
        private IEnumerable<MalzemeYeriTanim> GetDataFromDataSource()
        {
            // Code to fetch the data from the data source
            // ...
            var malzemeYeriList = _unitOfWork.MalzemeYeriTanim.GetAll();
            return  malzemeYeriList;
        }
        public async Task<JsonResult> GetAllMalzemeYeri(int malzemeId)
        {
            var malzemeYeriList = _unitOfWork.MalzemeYeriTanim.GetAll().ToList().Select(m => new
            {
                MalzemeYeriTanimd = m.Id,
                AdiWithAdet = m.Adi + " (" + _unitOfWork.MalzemeDagilim.GetAll()
                    .Where(md => md.MalzemeId == malzemeId && md.MalzemeYeriTanimId == m.Id)
                    .Sum(md => md.Adet) + ")"
            }).ToList();
            var malzemeYeriDropdownList = malzemeYeriList.Select(m => new SelectListItem
            {
                Value = m.MalzemeYeriTanimd.ToString(),
                Text = m.AdiWithAdet
            }).ToList();


            return Json(malzemeYeriDropdownList);
        }
        public async Task<JsonResult> GetMalzemeYeri(bool onlyExistingMalzeme,int malzemeId)
        {
            var malzemeYeriList = await _unitOfWork.MalzemeYeriTanim.GetMalzemeYeriDDL(onlyExistingMalzeme, malzemeId);
            return Json(malzemeYeriList);
        }
        //Delete
        [HttpPost]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _unitOfWork.MalzemeYeriTanim.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new {success=false, message="Kayıt silmede hata"});
            }
            _unitOfWork.MalzemeYeriTanim.Remove(obj);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Marka silindi" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MalzemeYeriTanim obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.Id == 0)
                {
                    string? userName = HttpContext.User.Identity.Name;
                    obj.Olusturan = userName;
                    _unitOfWork.MalzemeYeriTanim.Add(obj);
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
        public IActionResult Edit(MalzemeYeriTanim obj)
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
                    _unitOfWork.MalzemeYeriTanim.Update(obj);
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
