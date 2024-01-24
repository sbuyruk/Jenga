using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.MTS;
using Jenga.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class KisiController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public KisiController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {            
            return View();
        }
        
        public IActionResult GetAll()
        {
            //var objKisiList = _unitOfWork.Kisi.GetAll(includeProperties: "MTSKurumGorevs");
            var objKisiList = _unitOfWork.Kisi.IncludeIt();

            foreach (var item in objKisiList)
            {
                var list = new List<MTSKurumGorev>();
                item.Kutlama = item.Kutlama == null ? item.Kutlama = false : item.Kutlama;
                item.DogumTarihi = item.DogumTarihi == null ? item.DogumTarihi = DateTime.MinValue : item.DogumTarihi;
                if (item.MTSKurumGorevs != null)
                {

                    foreach (var kurumGorev in item.MTSKurumGorevs)
                    {
                        if ((kurumGorev.Durum !=null) && (kurumGorev.Durum.Equals(ProjectConstants.MTSGOREVDURUMU_GOREVDE)))
                        {
                            list.Add(kurumGorev);
                            break;
                        }
                    }

                }
                item.MTSKurumGorevs = list;
            }
            var aa = JsonConvert.SerializeObject(objKisiList, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
            var result = new JsonResult(JsonConvert.DeserializeObject(aa));

            //return Json(new { data = objKisiList });// result.Value });
            return Json(new { data = result.Value });
        }
        //GET
        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var kisiFromDb = _unitOfWork.Kisi.GetFirstOrDefault(u => u.Id == id);
            var mtsKurumGorevDb = _unitOfWork.MTSKurumGorev.GetFirstOrDefault(
                u => u.KisiId == id && !string.IsNullOrEmpty(u.Durum) && u.Durum.Equals(ProjectConstants.MTSGOREVDURUMU_GOREVDE),
                includeProperties:"MTSKurumTanim,MTSGorevTanim");
            if (kisiFromDb == null)
            {
                return NotFound();
            }
            KisiVM kisiVM = new()
            {
                Kisi = kisiFromDb,
                IlList = _unitOfWork.Il.GetAll().Select(i => new SelectListItem
                {
                    Text = i.IlAdi,
                    Value = i.Id.ToString()
                }),
                IlceList = _unitOfWork.Ilce.GetAll().Select(i => new SelectListItem
                {
                    Text = i.IlceAdi,
                    Value = i.Id.ToString()
                }),
                
                MTSUnvanTanimList = _unitOfWork.MTSUnvanTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.KisaAdi,
                    Value = i.Id.ToString()
                }),
                MTSKurumu = mtsKurumGorevDb != null ? mtsKurumGorevDb.MTSKurumTanim:null,
                MTSGorevi = mtsKurumGorevDb != null ? mtsKurumGorevDb.MTSGorevTanim:null,
            };
            return View(kisiVM);
        }
        [HttpGet]
        public IActionResult Create()
        {
            KisiVM kisiVM = new()
            {
                Kisi = new(),
                IlList = _unitOfWork.Il.GetAll().Select(i => new SelectListItem
                {
                    Text = i.IlAdi,
                    Value = i.Id.ToString()
                }).OrderBy(a => a.Text),
                IlceList = _unitOfWork.Ilce.GetAll().Select(i => new SelectListItem
                {
                    Text = i.IlceAdi,
                    Value = i.Id.ToString()
                }).OrderBy(a=> a.Text),
               
                MTSUnvanTanimList = _unitOfWork.MTSUnvanTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.KisaAdi,
                    Value = i.Id.ToString()
                }).OrderBy(a => a.Text),
                MTSKurumu = null,
                MTSGorevi = null,
            };

            return View(kisiVM);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(KisiVM obj)
        {


            if ((obj==null)|| (obj.Kisi==null) || string.IsNullOrEmpty(obj.Kisi.Adi.Trim()))
            {
                ModelState.AddModelError("adi", "Lütfen Kisi Adını boş bırakmayınız");
            }
            if (ModelState.IsValid)
            {
                string? userName = HttpContext.User.Identity.Name;
                obj.Kisi.Olusturan = userName;
                _unitOfWork.Kisi.Add(obj.Kisi);
                _unitOfWork.Save();
                TempData["success"] = "Kisi kayıtlara eklendi.";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Kayıt başarısız";
            }
            return View(obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(KisiVM obj)
        {
            if ((obj == null) || (obj.Kisi == null) || string.IsNullOrEmpty(obj.Kisi.Adi.Trim()))
            {
                ModelState.AddModelError("adi", "Lütfen Kisi Adını boş bırakmayınız");
            }
            if (ModelState.IsValid)
            {
                string? userName = HttpContext.User.Identity.Name;
                obj.Kisi.Degistiren = userName;
                _unitOfWork.Kisi.Update(obj.Kisi);
                _unitOfWork.Save();
                TempData["success"] = "Kişi kaydı güncellendi";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Kayıt başarısız";
            }
            return View(obj);
        }
        
       
        [HttpPost]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _unitOfWork.Kisi.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Kayıt silmede hata" });
            }
            var faaliyetKatilim = _unitOfWork.FaaliyetKatilim.GetByFilter(r => r.KatilimciId.Equals(obj.Id) && r.KatilimciTipi == ProjectConstants.RANDEVU_KATILIMCI_DIS_INT);
            if (faaliyetKatilim.Count > 0)
            {
                return Json(new { success = false, message = "Kayıt silinemez, faaliyet katılımı kaydı bulundu" });

            }
            else
            {
                //RandevuKatilim  kaydı varsa silinmesin       
                var disIrtibat = _unitOfWork.Faaliyet.GetByFilter(u => u.DisIrtibatId == obj.Id);
                if (disIrtibat.Count > 0)
                {
                    return Json(new { success = false, message = "Kayıt silinemez, Dış irtibat kaydı bulundu" });
                }
                else
                {
                    _unitOfWork.Kisi.Remove(obj);
                    KisiVM kisiVM = new()
                    {
                        Kisi = obj

                    };

                    string? userName = HttpContext.User.Identity.Name;
                    obj.Olusturan = userName;
                    _unitOfWork.Save();
                    return Json(new { success = true, message = "Kişi kaydı silindi" });
                }
                
            }

        }
        private bool SilinebilirMi(Kisi obj)
        {
            bool silinebilirMi = true;
            //RandevuKatilim  kaydı varsa silinmesin          
            var faaliyetKatilim = _unitOfWork.FaaliyetKatilim.GetByFilter(r => r.KatilimciId.Equals(obj.Id) && r.KatilimciTipi==ProjectConstants.RANDEVU_KATILIMCI_DIS_INT);
            if (faaliyetKatilim.Count > 0)
            {
                silinebilirMi = false;
            }
            else
            {
                //RandevuKatilim  kaydı varsa silinmesin       
                var disIrtibat = _unitOfWork.Faaliyet.GetByFilter(u => u.DisIrtibatId==obj.Id );
                if (disIrtibat.Count > 0)
                {
                    silinebilirMi = false;
                }
                else
                {

                }
            }
           
            //Arama kaydı varsa silinmesin
            //Anı Objesi verilmişse silinmesin
            return silinebilirMi;
        }
    }
}
