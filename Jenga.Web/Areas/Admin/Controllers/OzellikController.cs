using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.DYS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class OzellikController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public OzellikController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
            _cache = cache;
        }
        private readonly IMemoryCache _cache;
        public IActionResult Index()
        {
            IEnumerable<Ozellik> objOzellikList = _unitOfWork.Ozellik.GetAll();
            return View(objOzellikList);
        }

        //GET
        public IActionResult Create()
        {

            OzellikVM malzemeCinsiVM = new()
            {
                Ozellik = new(),

                MalzemeCinsiList = _unitOfWork.MalzemeCinsi.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),

            };

            return View(malzemeCinsiVM);

        }
        public IActionResult Edit(int? id)
        {
            OzellikVM malzemeCinsiVM = new()
            {
                Ozellik = new(),

                MalzemeCinsiList = _unitOfWork.MalzemeCinsi.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),

            };

            if (id == null || id == 0)
            {
                return NotFound();
            }
            else
            {
                //update 
                malzemeCinsiVM.Ozellik = _unitOfWork.Ozellik.GetFirstOrDefault(u => u.Id == id);
                return View(malzemeCinsiVM);
            }

        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var malzemeCinsiList = _unitOfWork.Ozellik.GetAll(includeProperties: "MalzemeCinsi");
            return Json(new { data = malzemeCinsiList });
            //return Json(new { data = cachedObject });
        }
        [HttpGet]
        public IActionResult GetByMalzemeCinsi(int malzemeCinsiId)
        {
            var malzemeCinsiList = _unitOfWork.Ozellik.GetByFilter(t => t.MalzemeCinsiId == malzemeCinsiId, includeProperties: "MalzemeCinsi");
            return Json(new { data = malzemeCinsiList });
            //return Json(new { data = cachedObject });
        }
        private IEnumerable<Ozellik> GetDataFromDataSource()
        {
            // Code to fetch the data from the data source
            // ...
            var malzemeCinsiList = _unitOfWork.Ozellik.GetAll(includeProperties: "MalzemeCinsi");
            return  malzemeCinsiList;
        }
        [HttpGet]
        public IActionResult GetAllByMalzemeCinsiId(int malzemeCinsiId, int malzemeId)
        {
            List<Ozellik> returnList = new List<Ozellik>();
            List<Ozellik> olist = _unitOfWork.Ozellik.GetByFilter(u => u.MalzemeCinsiId == malzemeCinsiId, includeProperties: "MalzemeCinsi").ToList();
            List<MalzemeOzellik> molist = _unitOfWork.MalzemeOzellik.GetByFilter(u => u.MalzemeId == malzemeId, includeProperties: "Malzeme,Ozellik").ToList();
            List<Ozellik> filteredList =olist.Where(o => !molist.Select(m => m.OzellikId).Contains(o.Id)).ToList();
            returnList.AddRange(filteredList);
            MalzemeCinsi malzemeCinsi = _unitOfWork.MalzemeCinsi.GetFirstOrDefault(u => u.Id == malzemeCinsiId);

            while (malzemeCinsi.UstMalzemeCinsiId >0 ) 
            {

                List<Ozellik> list = _unitOfWork.Ozellik.GetByFilter(u => u.MalzemeCinsiId == malzemeCinsi.UstMalzemeCinsiId, includeProperties: "MalzemeCinsi").ToList().Where(o => !molist.Select(m => m.OzellikId).Contains(o.Id)).ToList(); ;
                returnList.AddRange(list);
                malzemeCinsi = _unitOfWork.MalzemeCinsi.GetFirstOrDefault(u => u.Id == malzemeCinsi.UstMalzemeCinsiId);
            } 

            return Json(new { data = returnList });
        }
        //Delete
        [HttpPost]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _unitOfWork.Ozellik.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new {success=false, message="Kayıt silmede hata"});
            }
            _unitOfWork.Ozellik.Remove(obj);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Malzeme özelliği silindi" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(OzellikVM obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.Ozellik.Id == 0)
                {
                    string? userName = HttpContext.User.Identity.Name;
                    obj.Ozellik.Olusturan = userName;
                    _unitOfWork.Ozellik.Add(obj.Ozellik);
                    TempData["success"] = "Malzeme özelliği oluşturuldu";
                }
                else
                {
                    TempData["error"] = "Malzeme özelliği oluşturulamadı";
                }
                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(OzellikVM obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.Ozellik.Id == 0)
                {
                    TempData["success"] = "Malzeme özelliği Id bulunamadı";
                }
                else
                {
                    string? userName = HttpContext.User.Identity.Name;
                    obj.Ozellik.Degistiren = userName;
                    _unitOfWork.Ozellik.Update(obj.Ozellik);
                    TempData["success"] = "Malzeme özelliği güncellendi";
                }
                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }
        #endregion
    }


}
