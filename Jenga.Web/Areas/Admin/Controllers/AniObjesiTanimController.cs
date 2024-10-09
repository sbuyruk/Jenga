using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.DYS;
using Jenga.Models.MTS;
using Jenga.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class AniObjesiTanimController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public AniObjesiTanimController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
            _cache = cache;
        }
        private readonly IMemoryCache _cache;
        public IActionResult Index()
        {
            return View();
        }

        //GET
        public IActionResult Create()
        {
            var gorusmeSekliList = new List<SelectListItem> {
              new SelectListItem { Text = "Stoklu", Value = "Stoklu" },
              new SelectListItem { Text = "Stoksuz", Value = "Stoksuz" }
            };
            AniObjesiVM aniObjesiVM = new()
            {
                AniObjesiTanim = new(),
                KaynakTanimList = _unitOfWork.KaynakTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
                StokDurumuList = gorusmeSekliList

            };

            return View(aniObjesiVM);

        }
        public IActionResult Edit(int? id)
        {
            var stokDurumuList = new List<SelectListItem> {
              new SelectListItem { Text = "Stoklu", Value = "Stoklu" },
              new SelectListItem { Text = "Stoksuz", Value = "Stoksuz" }
            };
            AniObjesiVM aniObjesiVM = new()
            {
                AniObjesiTanim = new(),
                KaynakTanimList = _unitOfWork.KaynakTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
                StokDurumuList = stokDurumuList

            };

            if (id == null || id == 0)
            {
                return NotFound();
            }
            else
            {
                //update 
                aniObjesiVM.AniObjesiTanim = _unitOfWork.AniObjesiTanim.GetFirstOrDefault(u => u.Id == id);
                return View(aniObjesiVM);
            }

        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            // Check if the item is present in the cache
            if (!_cache.TryGetValue("aniObjesiListCache", out IEnumerable<AniObjesiTanim> cachedObject))
            {
                // Item is not in the cache, fetch it from the data source
                cachedObject = GetDataFromDataSource();

                // Set the item in the cache with a specific expiration time
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10)); // Example: Sliding expiration of 10 minutes

                _cache.Set("aniObjesiListCache", cachedObject, cacheOptions);
            }

            var aniObjesiList = _unitOfWork.AniObjesiTanim.GetAll(includeProperties: "KaynakTanim");
            return Json(new { data = aniObjesiList });
            //return Json(new { data = cachedObject });
        }
        private IEnumerable<AniObjesiTanim> GetDataFromDataSource()
        {
            // Code to fetch the data from the data source
            // ...
            var aniObjesiList = _unitOfWork.AniObjesiTanim.GetAll(includeProperties: "KaynakTanim");
            return  aniObjesiList;
        }
        //Delete
        [HttpPost]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _unitOfWork.AniObjesiTanim.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new {success=false, message="Kayıt silmede hata"});
            }
            _unitOfWork.AniObjesiTanim.Remove(obj);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Anı Objesi silindi" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AniObjesiVM obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.AniObjesiTanim.Id == 0)
                {
                    string? userName = HttpContext.User.Identity.Name;
                    obj.AniObjesiTanim.Olusturan = userName;
                    _unitOfWork.AniObjesiTanim.Add(obj.AniObjesiTanim);
                    TempData["success"] = "Anı Objesi oluşturuldu";
                }
                else
                {
                    TempData["error"] = "Anı Objesi oluşturulamadı";
                }
                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(AniObjesiVM obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.AniObjesiTanim.Id == 0)
                {
                    TempData["success"] = "Anı Objesi Id bulunamadı";
                }
                else
                {
                    string? userName = HttpContext.User.Identity.Name;
                    obj.AniObjesiTanim.Degistiren = userName;
                    _unitOfWork.AniObjesiTanim.Update(obj.AniObjesiTanim);
                    TempData["success"] = "Anı Objesi güncellendi";
                }
                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }
        #endregion
    }


}
