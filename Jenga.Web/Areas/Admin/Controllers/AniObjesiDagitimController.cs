using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.IKYS;
using Jenga.Models.MTS;
using Jenga.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class AniObjesiDagitimController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public AniObjesiDagitimController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment, IMemoryCache cache)
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
            var aniObjesiList = _unitOfWork.AniObjesiDagitim.GetAll(includeProperties: "AniObjesiTanim,Randevu,DepoTanim,DagitimYeriTanim");
            foreach (var item in aniObjesiList)
            {
                if (item.KatilimciTipi ==ProjectConstants.RANDEVU_KATILIMCI_DIS_INT)
                {
                    Kisi kisi = _unitOfWork.Kisi.GetFirstOrDefault(u=> u.Id==item.KatilimciId);
                    Katilimci katilimci = new Katilimci()
                    {
                        Id = item.KatilimciId,
                        Adi = kisi.Adi,
                        Aciklama = kisi.Aciklama,
                        Adres = kisi.Adres,
                        Dahili1 = kisi.Dahili1,
                        Dahili2 = kisi.Dahili2,
                        Dahili3 = kisi.Dahili3,
                        DogumTarihi = kisi.DogumTarihi,
                        Gorevi = kisi.Gorevi,
                        Ilcesi = kisi.Ilcesi,
                        Ili = kisi.Ili,
                        Kurumu = kisi.Kurumu,
                        Kutlama = kisi.Kutlama,
                        KatilimciTipi = item.KatilimciTipi,
                        Soyadi = kisi.Soyadi,
                        Unvani = kisi.Unvani,
                    };
                    item.Katilimci = katilimci;

                }
                else if(item.KatilimciTipi == ProjectConstants.RANDEVU_KATILIMCI_IC_INT)
                {
                    //Personel personel = _unitOfWork.Personel.GetFirstOrDefault(u => u.Id == item.KatilimciId, includeProperties: "IsBilgileri");
                    var personel = _unitOfWork.IsBilgileri.GetFirstOrDefault(u => u.Id == item.KatilimciId, includeProperties: "Personel");
                    Katilimci katilimci = new Katilimci()
                    {
                        Id = item.KatilimciId,
                        Adi = personel.Personel.Adi,
                        Aciklama = personel.Personel.Aciklama,
                        //Adres = personel.Adres,
                        //Dahili1 = personel.Dahili1,
                        //Dahili2 = personel.Dahili2,
                        //Dahili3 = personel.Dahili3,
                        //DogumTarihi = personel.DogumTarihi,
                        //Gorevi = personel.Gorevi,
                        //Ilcesi = personel.Ilcesi,
                        //Ili = personel.Ili,
                        //Kurumu = personel.Kurumu,
                        //Kutlama = personel.Kutlama,
                        KatilimciTipi = item.KatilimciTipi,
                        Soyadi = personel.Personel.Soyadi,
                        //Unvani = personel.Unvani,
                    };
                    item.Katilimci = katilimci;

                }
                else if (item.KatilimciTipi == ProjectConstants.RANDEVU_KATILIMCI_NAKITBAGISCI_INT)
                {
                    //Personel personel = _unitOfWork.Personel.GetFirstOrDefault(u => u.Id == item.KatilimciId, includeProperties: "IsBilgileri");
                    var nakitBagisci = _unitOfWork.IsBilgileri.GetFirstOrDefault(u => u.Id == item.KatilimciId, includeProperties: "Personel");
                    Katilimci katilimci = new Katilimci()
                    {
                        Id = item.KatilimciId,
                        Adi = nakitBagisci.Personel.Adi,
                        Aciklama = nakitBagisci.Personel.Aciklama,
                        //Adres = personel.Adres,
                        //Dahili1 = personel.Dahili1,
                        //Dahili2 = personel.Dahili2,
                        //Dahili3 = personel.Dahili3,
                        //DogumTarihi = personel.DogumTarihi,
                        //Gorevi = personel.Gorevi,
                        //Ilcesi = personel.Ilcesi,
                        //Ili = personel.Ili,
                        //Kurumu = personel.Kurumu,
                        //Kutlama = personel.Kutlama,
                        KatilimciTipi = item.KatilimciTipi,
                        Soyadi = nakitBagisci.Personel.Soyadi,
                        //Unvani = personel.Unvani,
                    };
                    item.Katilimci = katilimci;

                }
            }
            return Json(new { data = aniObjesiList });
            //return Json(new { data = cachedObject });
        }
        
        //Delete
        [HttpDelete]
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
