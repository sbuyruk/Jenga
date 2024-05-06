using Jenga.DataAccess.Repository;
using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.IKYS;
using Jenga.Models.MTS;
using Jenga.Utility;
using Jenga.Web.Areas.Admin.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        [HttpGet]
        public IActionResult Create(int kisiId)
        {

            var gorusmeSekliList = new List<SelectListItem> {
              new SelectListItem { Text = ProjectConstants.ARAMAGORUSME_GELENTELEFON, Value = ProjectConstants.ARAMAGORUSME_GELENTELEFON },
              new SelectListItem { Text = ProjectConstants.ARAMAGORUSME_GIDENTELEFON, Value = ProjectConstants.ARAMAGORUSME_GIDENTELEFON },
              new SelectListItem { Text = ProjectConstants.ARAMAGORUSME_YONETICIDIREKTIFI, Value = ProjectConstants.ARAMAGORUSME_YONETICIDIREKTIFI },
              new SelectListItem { Text = ProjectConstants.ARAMAGORUSME_YUZYUZEGORUSME, Value = ProjectConstants.ARAMAGORUSME_YUZYUZEGORUSME }
            };
            Kisi gorusulenKisi = _unitOfWork.Kisi.IncludeThis(kisiId);
            if (gorusulenKisi != null)
            {
                var kurum = string.Empty;
                if (gorusulenKisi.MTSKurumGorevs != null && gorusulenKisi.MTSKurumGorevs.Count > 0
                    && gorusulenKisi.MTSKurumGorevs[0].MTSKurumTanim != null && gorusulenKisi.MTSKurumGorevs[0].MTSKurumTanim.Adi != null)
                {
                    kurum = gorusulenKisi.MTSKurumGorevs[0].MTSKurumTanim.Adi;
                }
                var gorev = string.Empty;
                if (gorusulenKisi.MTSKurumGorevs != null && gorusulenKisi.MTSKurumGorevs.Count > 0
                    && gorusulenKisi.MTSKurumGorevs[0].MTSGorevTanim != null && gorusulenKisi.MTSKurumGorevs[0].MTSGorevTanim.Adi != null)
                {
                    gorev = gorusulenKisi.MTSKurumGorevs[0].MTSGorevTanim.Adi;
                }
                string kisiBilgisi = gorusulenKisi != null ? gorusulenKisi.Adi + " " + gorusulenKisi.Soyadi + " " + kurum + " " + gorev : "";
                AramaGorusmeVM aramaGorusmeVM = new()
                {

                    AramaGorusme = new()
                    {
                        Tarih = DateTime.Now,
                        ArayanId = gorusulenKisi == null ? 0 : gorusulenKisi.Id,
                    },
                    GorusmeSekliList = gorusmeSekliList,
                    GorusulenKisi = gorusulenKisi,
                    KisiBilgisi = kisiBilgisi,
                    KurumGorev = (kurum + " " + gorev).Trim(),
                };
                return View(aramaGorusmeVM);
            }
            else
            {
                AramaGorusmeVM aramaGorusmeVM = new()
                {

                    AramaGorusme = new()
                    {
                        Tarih = DateTime.Now,
                        ArayanId = gorusulenKisi == null ? 0 : gorusulenKisi.Id,
                    },
                    GorusmeSekliList = gorusmeSekliList,
                    GorusulenKisi = gorusulenKisi,
                    KisiBilgisi = string.Empty,
                    KurumGorev = string.Empty,
                };
                return View(aramaGorusmeVM);
            }
        }
        [HttpGet]
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
            var gorusmeSekliList = new List<SelectListItem> {
              new SelectListItem { Text = ProjectConstants.ARAMAGORUSME_GELENTELEFON, Value = ProjectConstants.ARAMAGORUSME_GELENTELEFON },
              new SelectListItem { Text = ProjectConstants.ARAMAGORUSME_GIDENTELEFON, Value = ProjectConstants.ARAMAGORUSME_GIDENTELEFON },
              new SelectListItem { Text = ProjectConstants.ARAMAGORUSME_YONETICIDIREKTIFI, Value = ProjectConstants.ARAMAGORUSME_YONETICIDIREKTIFI },
              new SelectListItem { Text = ProjectConstants.ARAMAGORUSME_YUZYUZEGORUSME, Value = ProjectConstants.ARAMAGORUSME_YUZYUZEGORUSME }
            };
            Kisi gorusulenKisi = _unitOfWork.Kisi.IncludeThis(aramaGorusmeFromDb.ArayanId);
            if (gorusulenKisi!=null)
            {
                var kurum = string.Empty;
                if (gorusulenKisi.MTSKurumGorevs!=null && gorusulenKisi.MTSKurumGorevs.Count >0 
                    && gorusulenKisi.MTSKurumGorevs[0].MTSKurumTanim != null && gorusulenKisi.MTSKurumGorevs[0].MTSKurumTanim.Adi != null)
                {
                    kurum = gorusulenKisi.MTSKurumGorevs[0].MTSKurumTanim.Adi;
                }
                var gorev = string.Empty;
                if (gorusulenKisi.MTSKurumGorevs != null && gorusulenKisi.MTSKurumGorevs.Count > 0 
                    && gorusulenKisi.MTSKurumGorevs[0].MTSGorevTanim != null && gorusulenKisi.MTSKurumGorevs[0].MTSGorevTanim.Adi != null)
                {
                    gorev = gorusulenKisi.MTSKurumGorevs[0].MTSGorevTanim.Adi;
                }
                string kisiBilgisi = gorusulenKisi != null ? gorusulenKisi.Adi + " " + gorusulenKisi.Soyadi + " " + kurum + " " + gorev : "";
                AramaGorusmeVM aramaGorusmeVM = new()
                {

                    AramaGorusme = aramaGorusmeFromDb,
                    GorusmeSekliList = gorusmeSekliList,
                    GorusulenKisi = gorusulenKisi,
                    KisiBilgisi = kisiBilgisi,
                    KurumGorev = (kurum + " " + gorev).Trim(),
                }; 
            return View(aramaGorusmeVM);
            }
            else
            {
                return NotFound();
            }
            
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
        public IActionResult Edit(AramaGorusmeVM obj)
        {
            if (ModelState.IsValid)
            {
                string? userName = HttpContext.User.Identity.Name;
                obj.AramaGorusme.Degistiren = userName;
                _unitOfWork.AramaGorusme.Update(obj.AramaGorusme);
                _unitOfWork.Save();
                TempData["success"] = "Arama/Görüşme başarıyla güncellendi.";
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
            if (aramaGorusmeToBeDeleted.FaaliyetId > 0)
            {
                return Json(new { success = false, message = "Bu arama/görüşme ile ilişkilendirilmiş bir faaliyet bulunmaktadır." });
            }
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
        //[HttpPost]
        public IActionResult CreateFaaliyet(int id)
        {
            if (ModelState.IsValid)
            {
                AramaGorusme aramaGorusme = _unitOfWork.AramaGorusme.GetFirstOrDefault(u => u.Id == id);
                if (aramaGorusme == null)
                {
                    TempData["error"] = "Arama/Görüşme kaydı bulunamadığından Faaliyet oluşturulamadı.";
                    return RedirectToAction("Index");
                }
                string? userName = HttpContext.User.Identity.Name;
                Faaliyet faaliyet = new()
                {
                    Aciklama = id + " numaralı Arama/Görşme kaydı ile otomatik oluşturuldu.<br>" + aramaGorusme.Aciklama,
                    AcikTarih = true,
                    BaslangicTarihi = DateTime.Now,
                    BitisTarihi = DateTime.Now.AddMinutes(30),

                    DisIrtibatId = aramaGorusme.ArayanId,
                    FaaliyetAmaci = int.Parse(ProjectConstants.FAALIYET_AMACI_GORUSME_INT),
                    FaaliyetYeriStr = ProjectConstants.FAALIYET_YERI_GMMAKAMI,
                    FaaliyetTipi = ProjectConstants.RANDEVU_VERILEN,
                    FaaliyetDurumu = ProjectConstants.FAALIYET_DURUMU_PLANLANDI_INT,
                    FaaliyetKonusu = aramaGorusme.Konu,
                    Olusturan = userName,
                    TakvimeIslendi = false,
                    TumGun = false,

                };
                _unitOfWork.Faaliyet.Add(faaliyet);
                TempData["success"] = "Arama/Görüşme bağlantılı faaliyet kaydı oluşturuldu";
                _unitOfWork.Save();
                aramaGorusme.FaaliyetId=faaliyet.Id;
                _unitOfWork.AramaGorusme.Update(aramaGorusme);
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return View();
        }
        [HttpGet]
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
        //[HttpGet]
        public IActionResult GetAllAramaGorusmeList()
        {
            //if (baslangicTarihi == null || baslangicTarihi < ProjectConstants.ILK_TARIH)
            //{
            //    baslangicTarihi = DateTime.Today.AddMonths(-5);
            //}

            // select all the AramaGorusme rows from the database into a list
            //var aramaGorusme = _unitOfWork.AramaGorusme.GetByFilter(a=> a.Tarih >= baslangicTarihi, includeProperties: "Faaliyet");

            //var aramaGorusme = _katilimciService.GetAllAramaGorusmeWithKatilimci();

            //var aramaGorusmeList = _unitOfWork.AramaGorusme.GetByFilter(a => a.Tarih >= baslangicTarihi);
            //var faaliyetList = _unitOfWork.Faaliyet.GetAll();

            //var list1 = from aramaGorusme in aramaGorusmeList
            //             join faaliyet in faaliyetList
            //             on aramaGorusme.FaaliyetId equals faaliyet.Id into childGroup
            //             from child in childGroup.DefaultIfEmpty()
            //             select new { aramaGorusme };
            //var list = list1
            //          .Select(a => new
            //          {
            //              AramaGorusme = a.aramaGorusme,
            //              Katilimci = _katilimciService.GetKatilimci(a.aramaGorusme.ArayanId, a.aramaGorusme.KatilimciTipi)


            //          });
            var faaliyetList = _unitOfWork.Faaliyet.GetAll();
            var kisiList = _unitOfWork.Kisi.IncludeIt();
            var aramaGorusmeList = _unitOfWork.AramaGorusme.GetAll();///includeProperties: "Kisi");
            var list1 = from aramaGorusme in aramaGorusmeList
                        join faaliyet in faaliyetList
                        on aramaGorusme.FaaliyetId equals faaliyet.Id into childGroup
                        from child in childGroup.DefaultIfEmpty()
                        join kisi in kisiList
                        on aramaGorusme.ArayanId equals kisi.Id into childGroup1
                        from child1 in childGroup1.DefaultIfEmpty()
                        select new { aramaGorusme };

            var aa = JsonConvert.SerializeObject(list1, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
            var result = new JsonResult(JsonConvert.DeserializeObject(aa));

            return Json(new { data = result.Value });
        }
       
        #endregion
        
    }
}
