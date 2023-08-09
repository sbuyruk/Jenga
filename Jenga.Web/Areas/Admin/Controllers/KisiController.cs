using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.MTS;
using Jenga.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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
            var objKisiList = _unitOfWork.Kisi.GetAll();
            //var settings = new JsonSerializerSettings
            //{
            //    NullValueHandling = NullValueHandling.Ignore
            //};
            //return Json(new { data = objKisiList }, settings);
            foreach (var item in objKisiList)
            {
                item.Kutlama= item.Kutlama == null ? item.Kutlama = false : item.Kutlama;
                item.DogumTarihi= item.DogumTarihi== null ? item.DogumTarihi = DateTime.MinValue : item.DogumTarihi;
            }
            return Json(new { data = objKisiList });
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
                }),
                IlceList = _unitOfWork.Ilce.GetAll().Select(i => new SelectListItem
                {
                    Text = i.IlceAdi,
                    Value = i.Id.ToString()
                }),
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
                TempData["success"] = "Kisi kaydı güncellendi";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Kayıt başarısız";
            }
            return View(obj);
        }
        
       
        [HttpDelete]
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
            var randevuKatilim = _unitOfWork.RandevuKatilim.GetByFilter(r => r.KatilimciId.Equals(obj.Id) && r.KatilimciTipi == ProjectConstants.RANDEVU_KATILIMCI_DIS_INT);
            if (randevuKatilim.Count > 0)
            {
                return Json(new { success = false, message = "Kayıt silinemez, randevu katılımı kaydı bulundu" });

            }
            else
            {
                //RandevuKatilim  kaydı varsa silinmesin       
                var disIrtibat = _unitOfWork.Randevu.GetByFilter(u => u.DisIrtibatId == obj.Id);
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
            var randevuKatilim = _unitOfWork.RandevuKatilim.GetByFilter(r => r.KatilimciId.Equals(obj.Id) && r.KatilimciTipi==ProjectConstants.RANDEVU_KATILIMCI_DIS_INT);
            if (randevuKatilim.Count > 0)
            {
                silinebilirMi = false;
            }
            else
            {
                //RandevuKatilim  kaydı varsa silinmesin       
                var disIrtibat = _unitOfWork.Randevu.GetByFilter(u => u.DisIrtibatId==obj.Id );
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
