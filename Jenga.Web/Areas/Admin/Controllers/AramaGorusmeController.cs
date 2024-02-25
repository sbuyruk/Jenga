using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.IKYS;
using Jenga.Models.MTS;
using Jenga.Utility;
using Jenga.Web.Areas.Admin.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class AramaGorusmeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly KatilimciService _katilimciService;

        public AramaGorusmeController(IUnitOfWork unitOfWork, KatilimciService katilimciService)
        {
            _unitOfWork = unitOfWork;
            _katilimciService = katilimciService;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create(int katilimciId, int katilimciTipi)
        {

            var gorusmeSekliList = new List<SelectListItem> {
              new SelectListItem { Text = ProjectConstants.ARAMAGORUSME_GELENTELEFON, Value = ProjectConstants.ARAMAGORUSME_GELENTELEFON },
              new SelectListItem { Text = ProjectConstants.ARAMAGORUSME_GIDENTELEFON, Value = ProjectConstants.ARAMAGORUSME_GIDENTELEFON },
              new SelectListItem { Text = ProjectConstants.ARAMAGORUSME_YONETICIDIREKTIFI, Value = ProjectConstants.ARAMAGORUSME_YONETICIDIREKTIFI },
              new SelectListItem { Text = ProjectConstants.ARAMAGORUSME_YUZYUZEGORUSME, Value = ProjectConstants.ARAMAGORUSME_YUZYUZEGORUSME }
            };
            Katilimci gorusulenKatilimci = _katilimciService.GetKatilimci(katilimciId, katilimciTipi);
            string katilimciBilgisi = gorusulenKatilimci != null ? gorusulenKatilimci.Adi + " " + gorusulenKatilimci.Soyadi + " " + gorusulenKatilimci.Kurumu + " " + gorusulenKatilimci.Gorevi : "";
            AramaGorusmeVM aramaGorusmeVM = new()
            {
                
                AramaGorusme = new() {
                    Tarih = DateTime.Now,
                    ArayanId = gorusulenKatilimci==null?0:gorusulenKatilimci.Id,
                    KatilimciTipi = gorusulenKatilimci==null?0:gorusulenKatilimci.KatilimciTipi.Value,
                },
                GorusmeSekliList = gorusmeSekliList,
                GorusulenKatilimci = gorusulenKatilimci,
                KatilimciBilgisi = katilimciBilgisi,
            };

            return View(aramaGorusmeVM);
            

        }
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            AramaGorusme? aramaGorusmeFromDb = _unitOfWork.AramaGorusme.GetFirstOrDefault(u => u.Id == id);
 
            if (aramaGorusmeFromDb == null)
            {
                return NotFound();
            }
            return View(aramaGorusmeFromDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AramaGorusmeVM obj)
        {

            if (ModelState.IsValid)
            {
                if (obj.AramaGorusme.Id == 0)
                {
                    string? userName = HttpContext.User.Identity.Name;
                    obj.AramaGorusme.Olusturan = userName;
                    _unitOfWork.AramaGorusme.Add(obj.AramaGorusme);
                    TempData["success"] = "Arama/Görüşme kaydı oluşturuldu";
                }
                else
                {
                    TempData["error"] = "Arama/Görüşme kaydı oluşturulamadı";
                }
                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
            
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(AramaGorusme obj)
        {
            if (ModelState.IsValid)
            {
                string? userName = HttpContext.User.Identity.Name;
                obj.Degistiren = userName;
                _unitOfWork.AramaGorusme.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Arama/Gorusme başarıyla güncellendi.";
                return RedirectToAction("Index");
            }
            return View();
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<AramaGorusme> list = _unitOfWork.AramaGorusme.GetAll().ToList();
            return Json(new { data = list });
        }

        [HttpPost]
        public IActionResult Delete(int? id)
        {
            var aramaGorusmeToBeDeleted = _unitOfWork.AramaGorusme.GetFirstOrDefault(u => u.Id == id);
            
            if (aramaGorusmeToBeDeleted == null)
            {
                return Json(new { success = false, message = "AramaGorusme  Bulunamadı." });
            }
            else
            {
                _unitOfWork.AramaGorusme.Remove(aramaGorusmeToBeDeleted);
                _unitOfWork.Save();

                return Json(new { success = true, message = "AramaGorusme  başarıyla silindi." }); 
            }

        }

        public IActionResult GetAllByDate(DateTime tarih)
        {
            tarih= tarih<ProjectConstants.ILK_TARIH? tarih:DateTime.Today.AddMonths(-3);
            var objAramaGorusmeList = _unitOfWork.AramaGorusme.GetByFilter(t=>t.Tarih>= tarih);

            var aa = JsonConvert.SerializeObject(objAramaGorusmeList, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
            var result = new JsonResult(JsonConvert.DeserializeObject(aa));

            return Json(new { data = result.Value });
        }

        public IActionResult GetAllAramaGorusmeWithKatilimci(DateTime? baslangicTarihi)
        {
            if (baslangicTarihi == null || baslangicTarihi < ProjectConstants.ILK_TARIH)
            {
                baslangicTarihi = DateTime.Today.AddMonths(-5);
            }
            var aramaGorusme = _unitOfWork.AramaGorusme.GetByFilter(a=> a.Tarih >= baslangicTarihi, includeProperties: "Faaliyet");
            //var aramaGorusme = _katilimciService.GetAllAramaGorusmeWithKatilimci();

            var list = aramaGorusme
                      .Select(a => new
                      {
                          AramaGorusme = a,
                          Katilimci = _katilimciService.GetKatilimci(a.ArayanId,(int)a.KatilimciTipi)
                            

                      }); ;

            return Json(new { data = list });
        }
        public IActionResult GetAllKatilimci()
        {

            var list = _katilimciService.GetAllKatilimci();            
            return Json(new { data = list });
        }
        #endregion

    }
}
