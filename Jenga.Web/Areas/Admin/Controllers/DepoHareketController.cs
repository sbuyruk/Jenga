using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models;
using Jenga.Models.MTS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class DepoHareketController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public DepoHareketController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }
        
        //GET
        public IActionResult Create()
        {
            var girisCikisList = new List<SelectListItem> {
              new SelectListItem { Text = "Giriş", Value = "Giriş" },
              new SelectListItem { Text = "Çıkış", Value = "Çıkış" }
            };
            DepoHareketVM depoHareketVM = new()
            {
                DepoHareket = new(),
                DepoTanimList = _unitOfWork.DepoTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
                AniObjesiList = _unitOfWork.AniObjesiTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
                GirisCikisList = girisCikisList

            };

            return View(depoHareketVM);
            
        }
        public IActionResult Edit(int? id)
        {
            var girisCikisList = new List<SelectListItem> {
              new SelectListItem { Text = "Giriş", Value = "Giriş" },
              new SelectListItem { Text = "Çıkış", Value = "Çıkış" }
            };
            DepoHareketVM depoHareketVM = new()
            {
                DepoHareket = new(),
                DepoTanimList = _unitOfWork.DepoTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
                AniObjesiList = _unitOfWork.AniObjesiTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
                GirisCikisList = girisCikisList

            };

            if (id == null || id == 0)
            {
                return NotFound();

            }
            else
            {
                //update 
                depoHareketVM.DepoHareket = _unitOfWork.DepoHareket.GetFirstOrDefault(u => u.Id == id);
                return View(depoHareketVM);
            }

        }
        

        private void DepoStokKaydiOlusturVeyaGuncelle(DepoHareketVM obj, bool isDelete)
        {
            //depoHareket_table daki kayitli adeti bul
            var depoHareketTabledaKayitliDepoHareket = _unitOfWork.DepoHareket.GetFirstOrDefault(dh => dh.Id == obj.DepoHareket.Id);
            int dbDeKayitliDepoHareketAdedi = depoHareketTabledaKayitliDepoHareket==null?0: depoHareketTabledaKayitliDepoHareket.Adet;
            //depoHareket_table daki kayitli adet ile güncellenen adet arasındaki farkı bul
            int fark = obj.DepoHareket.Adet - dbDeKayitliDepoHareketAdedi;
            if (isDelete)
            {
                fark = dbDeKayitliDepoHareketAdedi*-1;
            }
            //DepoStok_table'daki sonAdedi bul
            var depoStokTabledakiKayit = _unitOfWork.DepoStok.GetFirstOrDefault(dh => ((dh.DepoId == obj.DepoHareket.DepoId) && (dh.AniObjesiId == obj.DepoHareket.AniObjesiId)));
            // eğer DepoStok_Table'da bu depoda&&buAniObjesi kaydı yoksa yenisini yarat
            if (depoStokTabledakiKayit == null)
            {
                // yeni kayıt oluştur
                DepoStok depoStok = new()
                {
                    DepoId = obj.DepoHareket.DepoId,
                    AniObjesiId = obj.DepoHareket.AniObjesiId,
                    SonAdet = fark,
                    SonIslemYapan = "bıdı bıdı",
                    SonIslemTarihi = DateTime.Now,
                    Aciklama = obj.DepoHareket.Id + " kayıt numaralı depo hareketi ile otomatik oluşturuldu",
                    Olusturan = "bıdı bıdı nın emmoğlu",
                    OlusturmaTarihi = DateTime.Now,
                };
                _unitOfWork.DepoStok.Add(depoStok);
            }
            else
            {
                //DepoStok_table'daki sonAdede farkı ekle
                int dbDeKayitliDepoStokAdedi = depoStokTabledakiKayit.SonAdet;
                depoStokTabledakiKayit.SonAdet = dbDeKayitliDepoStokAdedi + fark;
                depoStokTabledakiKayit.SonIslemYapan = "Bu adam";
                depoStokTabledakiKayit.SonIslemTarihi = DateTime.Now;
                depoStokTabledakiKayit.Aciklama = " son adet güncellendi";
                depoStokTabledakiKayit.Degistiren = " burası Identity den gelecek";
                depoStokTabledakiKayit.DegistirmeTarihi = DateTime.Now;

                //DepoStok_Table'da güncelle
                _unitOfWork.DepoStok.Update(depoStokTabledakiKayit);
            }
            
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var depoHareketList = _unitOfWork.DepoHareket.GetAll(includeProperties:"DepoTanim,AniObjesiTanim");
            return Json(new { data = depoHareketList });
        }

        //Delete
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _unitOfWork.DepoHareket.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new {success=false, message="Kayıt silmede hata"});
            }

            _unitOfWork.DepoHareket.Remove(obj);
            DepoHareketVM depoHareketVM = new()
            {
                DepoHareket = obj
               
            };
            DepoStokKaydiOlusturVeyaGuncelle(depoHareketVM,true);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Depo işlemi silindi" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(DepoHareketVM obj)
        {
            if (ModelState.IsValid)
            {
                DepoStokKaydiOlusturVeyaGuncelle(obj, false);
                if (obj.DepoHareket.Id == 0)
                {
                    _unitOfWork.DepoHareket.Add(obj.DepoHareket);
                    TempData["success"] = "Depo işlemi gerçekleşti";
                }
                else
                {
                    TempData["error"] = "Depo Id bulunamadı";
                }

                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(DepoHareketVM obj)
        {
            if (ModelState.IsValid)
            {
                DepoStokKaydiOlusturVeyaGuncelle(obj, false);
                if (obj.DepoHareket.Id == 0)
                {
                    TempData["error"] = "Depo Id bulunamadı";
                }
                else
                {
                    _unitOfWork.DepoHareket.Update(obj.DepoHareket);
                    TempData["success"] = "Depo işlemi güncellendi";
                }

                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }
        #endregion

    }


}
