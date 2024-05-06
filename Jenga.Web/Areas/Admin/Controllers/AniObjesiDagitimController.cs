using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.IKYS;
using Jenga.Models.MTS;
using Jenga.Utility;
using Jenga.Web.Areas.Admin.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class AniObjesiDagitimController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly KatilimciService _katilimciService;
        public AniObjesiDagitimController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment, IMemoryCache cache, KatilimciService katilimciService)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
            _cache = cache;
            _katilimciService = katilimciService;
        }
        private readonly IMemoryCache _cache;
        public IActionResult Index()
        {
            return View();
        }

        //GET
        [HttpGet]
        public IActionResult Create()
        {


            AniObjesiDagitimVM aniObjesiDagitimVM = new()
            {
                AniObjesiDagitim = new(),
                AniObjesiTanimList = _unitOfWork.AniObjesiTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),

                DepoTanimList = _unitOfWork.DepoTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
            };
            
            return View(aniObjesiDagitimVM);

        }
        public IActionResult Edit(int? id)
        {
            var verilenAlinanList = new List<SelectListItem> {
              new SelectListItem { Text = "Verilen", Value = "False" },
              new SelectListItem { Text = "Alinan", Value = "True" }
            };
            AniObjesiDagitimVM aniObjesiDagitimVM = new()
            {
                AniObjesiDagitim = new(),
                AniObjesiTanimList = _unitOfWork.AniObjesiTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
                VerilenAlinanList = verilenAlinanList,
                DepoTanimList = _unitOfWork.DepoTanim.GetAll().Select(i => new SelectListItem
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
                aniObjesiDagitimVM.AniObjesiDagitim = _unitOfWork.AniObjesiDagitim.GetFirstOrDefault(u => u.Id == id);
                return View(aniObjesiDagitimVM);
            }

        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var aniObjesiList = _unitOfWork.AniObjesiDagitim.GetAll(includeProperties: "AniObjesiTanim,Faaliyet,DepoTanim,DagitimYeriTanim");
            foreach (var item in aniObjesiList)
            {
                if (item.KatilimciTipi == ProjectConstants.FAALIYET_KATILIMCI_DIS_INT)
                {

                    Katilimci katilimci = _katilimciService.GetKatilimci(item.KatilimciId, ProjectConstants.FAALIYET_KATILIMCI_DIS_INT);
                    item.Katilimci = katilimci;

                }
                else if (item.KatilimciTipi == ProjectConstants.FAALIYET_KATILIMCI_IC_INT)
                {
                    Katilimci katilimci = _katilimciService.GetKatilimci(item.KatilimciId, ProjectConstants.FAALIYET_KATILIMCI_IC_INT);
                    item.Katilimci = katilimci;

                }
                else if (item.KatilimciTipi == ProjectConstants.FAALIYET_KATILIMCI_NAKITBAGISCI_INT)
                {
                    Katilimci katilimci = _katilimciService.GetKatilimci(item.KatilimciId, ProjectConstants.FAALIYET_KATILIMCI_NAKITBAGISCI_INT);
                    item.Katilimci = katilimci;

                }
                else if (item.KatilimciTipi == ProjectConstants.FAALIYET_KATILIMCI_TASINMAZBAGISCI_INT)
                {
                    Katilimci katilimci = _katilimciService.GetKatilimci(item.KatilimciId, ProjectConstants.FAALIYET_KATILIMCI_TASINMAZBAGISCI_INT);
                    item.Katilimci = katilimci;

                }
            }
            return Json(new { data = aniObjesiList });
            
        }
        
        //Delete
        [HttpPost]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _unitOfWork.AniObjesiDagitim.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new {success=false, message="Kayıt silmede hata"});
            }
            _unitOfWork.AniObjesiDagitim.Remove(obj);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Anı Objesi Dağıtım silindi" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AniObjesiDagitimVM obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.AniObjesiDagitim.Id == 0)
                {
                    string? userName = HttpContext.User.Identity.Name;
                    obj.AniObjesiDagitim.Olusturan = userName;
                    _unitOfWork.AniObjesiDagitim.Add(obj.AniObjesiDagitim);
                    TempData["success"] = "Anı Objesi Dağıtım oluşturuldu";
                }
                else
                {
                    TempData["error"] = "Anı Objesi Dağıtım oluşturulamadı";
                }
                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(AniObjesiDagitimVM obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.AniObjesiDagitim.Id == 0)
                {
                    TempData["success"] = "Anı Objesi Dağıtım Id bulunamadı";
                }
                else
                {
                    string? userName = HttpContext.User.Identity.Name;
                    obj.AniObjesiDagitim.Degistiren = userName;
                    _unitOfWork.AniObjesiDagitim.Update(obj.AniObjesiDagitim);
                    TempData["success"] = "Anı Objesi Dağıtımı güncellendi";
                }
                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }
        #endregion
    }


}
