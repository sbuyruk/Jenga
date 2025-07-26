using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.MTS;
using Jenga.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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

        #region GET
        //GET
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult TransferIndex()
        {
            return View();
        }
        public IActionResult Create()
        {
            var girisCikisList = new List<SelectListItem> {
              new SelectListItem { Text = "Giriş", Value = "Giriş" },
              //new SelectListItem { Text = "Çıkış", Value = "Çıkış" } Depoya sadece giriş olsun
            };

            DepoHareketVM depoHareketVM = new()
            {
                DepoHareket = new(),
                AniObjesiList = _unitOfWork.AniObjesiTanim.GetByFilter(u => u.StokluMu == ProjectConstants.MTS_ANIOBJESISTOKLU).Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
                KaynakList = _unitOfWork.KaynakTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
                KaynakDepoList = _unitOfWork.DepoTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
                HedefDepoList = _unitOfWork.DepoTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
                GirisCikisList = girisCikisList

            };

            return View(depoHareketVM);

        }
        public IActionResult Transfer()
        {
            var girisCikisList = new List<SelectListItem> {
              new SelectListItem { Text = "Giriş", Value = "Giriş" },
              new SelectListItem { Text = "Çıkış", Value = "Çıkış" }
            };
            DepoHareketVM depoHareketVM = new()
            {
                DepoHareket = new(),
                AniObjesiList = _unitOfWork.AniObjesiTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
                KaynakList = _unitOfWork.KaynakTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
                KaynakDepoList = _unitOfWork.DepoTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
                HedefDepoList = _unitOfWork.DepoTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
                GirisCikisList = girisCikisList,
            };


            return View(depoHareketVM);

        }
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();

            }
            else
            {
                var girisCikisList = new List<SelectListItem> {
              new SelectListItem { Text = "Giriş", Value = "Giriş" },
              new SelectListItem { Text = "Çıkış", Value = "Çıkış" }
            };
                DepoHareketVM depoHareketVM = new()
                {
                    AniObjesiList = _unitOfWork.AniObjesiTanim.GetAll().Select(i => new SelectListItem
                    {
                        Text = i.Adi,
                        Value = i.Id.ToString()
                    }),
                    KaynakList = _unitOfWork.KaynakTanim.GetAll().Select(i => new SelectListItem
                    {
                        Text = i.Adi,
                        Value = i.Id.ToString()
                    }),
                    KaynakDepoList = _unitOfWork.DepoTanim.GetAll().Select(i => new SelectListItem
                    {
                        Text = i.Adi,
                        Value = i.Id.ToString()
                    }),
                    HedefDepoList = _unitOfWork.DepoTanim.GetAll().Select(i => new SelectListItem
                    {
                        Text = i.Adi,
                        Value = i.Id.ToString()
                    }),
                    GirisCikisList = girisCikisList

                };
                //update 
                depoHareketVM.DepoHareket = _unitOfWork.DepoHareket.GetFirstOrDefault(u => u.Id == id);
                return View(depoHareketVM);
            }

        }
        #endregion

        //private void DepoStokKaydiOlusturVeyaGuncelle(DepoHareketVM obj, bool isDelete)
        //{
        //    //depoHareket_table daki kayitli adeti bul
        //    var depoHareketTabledaKayitliDepoHareket = _unitOfWork.DepoHareket.GetFirstOrDefault(dh => dh.Id == obj.DepoHareket.Id);
        //    int dbDeKayitliDepoHareketAdedi = depoHareketTabledaKayitliDepoHareket==null?0: depoHareketTabledaKayitliDepoHareket.Adet;
        //    //depoHareket_table daki kayitli adet ile güncellenen adet arasındaki farkı bul
        //    int fark = obj.DepoHareket.Adet - dbDeKayitliDepoHareketAdedi;
        //    if (isDelete)
        //    {
        //        fark = dbDeKayitliDepoHareketAdedi*-1;
        //    }
        //    //DepoStok_table'daki sonAdedi bul
        //    var depoStokTabledakiKayit = _unitOfWork.DepoStok.GetFirstOrDefault(dh => ((dh.DepoId == obj.DepoHareket.KaynakDepoId) && (dh.AniObjesiId == obj.DepoHareket.AniObjesiId)));
        //    // eğer DepoStok_Table'da bu depoda&&buAniObjesi kaydı yoksa yenisini yarat
        //    if (depoStokTabledakiKayit == null)
        //    {
        //        // yeni kayıt oluştur
        //        DepoStok depoStok = new()
        //        {
        //            DepoId = obj.DepoHareket.KaynakDepoId,
        //            AniObjesiId = obj.DepoHareket.AniObjesiId,
        //            SonAdet = fark,
        //            SonIslemYapan = "bıdı bıdı",
        //            SonIslemTarihi = DateTime.Now,
        //            Aciklama = obj.DepoHareket.Id + " kayıt numaralı depo hareketi ile otomatik oluşturuldu",
        //            Olusturan = "bıdı bıdı nın emmoğlu",
        //            OlusturmaTarihi = DateTime.Now,
        //        };
        //        _unitOfWork.DepoStok.Add(depoStok);
        //    }
        //    else
        //    {
        //        //DepoStok_table'daki sonAdede farkı ekle
        //        int dbDeKayitliDepoStokAdedi = depoStokTabledakiKayit.SonAdet;
        //        depoStokTabledakiKayit.SonAdet = dbDeKayitliDepoStokAdedi + fark;
        //        depoStokTabledakiKayit.SonIslemYapan = "Bu adam";
        //        depoStokTabledakiKayit.SonIslemTarihi = DateTime.Now;
        //        depoStokTabledakiKayit.Aciklama = " son adet güncellendi";
        //        depoStokTabledakiKayit.Degistiren = " burası Identity den gelecek";
        //        depoStokTabledakiKayit.DegistirmeTarihi = DateTime.Now;

        //        //DepoStok_Table'da güncelle
        //        _unitOfWork.DepoStok.Update(depoStokTabledakiKayit);
        //    }

        //}
        private void DepoStoktanCikisAdediDus(DepoHareketVM obj)
        {
            bool isSuccess = false;
            //DepoStok_table'daki sonAdedi bul
            var depoStokTabledakiCikisDepoKaydi = _unitOfWork.DepoStok.GetFirstOrDefault(dh => ((dh.DepoId == obj.DepoHareket.KaynakDepoId) && (dh.AniObjesiId == obj.DepoHareket.AniObjesiId)));
            // eğer DepoStok_Table'da bu depoda&&buAniObjesi kaydı yoksa hata ver
            if (depoStokTabledakiCikisDepoKaydi == null)
            {
                //burada kaldım
            }
            else
            {

                //DepoStok_table'daki sonAdede farkı ekle
                int dbDeKayitliDepoStokAdedi = depoStokTabledakiCikisDepoKaydi.SonAdet;
                int aktarilanAdet = obj.DepoHareket.Adet;
                if (dbDeKayitliDepoStokAdedi < aktarilanAdet)
                {
                    //hata ver depo adedinden fazla aktarılamaz
                }
                else
                {
                    depoStokTabledakiCikisDepoKaydi.SonAdet = dbDeKayitliDepoStokAdedi - aktarilanAdet;
                    depoStokTabledakiCikisDepoKaydi.SonIslemYapan = "Bu adam";
                    depoStokTabledakiCikisDepoKaydi.SonIslemTarihi = DateTime.Now;
                    depoStokTabledakiCikisDepoKaydi.Aciklama = " son adet güncellendi";
                    depoStokTabledakiCikisDepoKaydi.Degistiren = " burası Identity den gelecek";
                    depoStokTabledakiCikisDepoKaydi.DegistirmeTarihi = DateTime.Now;
                }


                //DepoStok_Table'da güncelle
                _unitOfWork.DepoStok.Update(depoStokTabledakiCikisDepoKaydi);
            }
        }
        private void DepoStokaGirisAdediEkle(DepoHareketVM obj)
        {
            //DepoStok_table'daki sonAdedi bul
            var girisDepoStok = _unitOfWork.DepoStok.GetFirstOrDefault(dh => ((dh.DepoId == obj.DepoHareket.HedefDepoId) && (dh.AniObjesiId == obj.DepoHareket.AniObjesiId)));
            // eğer DepoStok_Table'da bu depoda&&buAniObjesi kaydı yoksa hata ver
            if (girisDepoStok == null)
            {
                DepoStok yeniDepoStok = new()
                {
                    Aciklama = obj.DepoHareket.Aciklama,
                    AniObjesiId = obj.DepoHareket.AniObjesiId,
                    DepoId = obj.DepoHareket.HedefDepoId,
                    Olusturan = HttpContext.User?.Identity?.Name,
                    SonAdet = obj.DepoHareket.Adet,
                    SonIslemTarihi = DateTime.Now,
                    SonIslemYapan = HttpContext.User?.Identity?.Name?.Split('\\')[1],
                };
                _unitOfWork.DepoStok.Update(yeniDepoStok);
            }
            else
            {

                //DepoStok_table'daki sonAdede farkı ekle
                int dbDeKayitliDepoStokAdedi = girisDepoStok.SonAdet;
                int aktarilanAdet = obj.DepoHareket.Adet;
                if (dbDeKayitliDepoStokAdedi < aktarilanAdet)
                {
                    //hata ver depo adedinden fazla aktarılamaz
                }
                else
                {
                    girisDepoStok.SonAdet = dbDeKayitliDepoStokAdedi + aktarilanAdet;
                    girisDepoStok.SonIslemYapan = "Bu adam";
                    girisDepoStok.SonIslemTarihi = DateTime.Now;
                    girisDepoStok.Aciklama = " son adet güncellendi";
                    girisDepoStok.Degistiren = " burası Identity den gelecek";
                    girisDepoStok.DegistirmeTarihi = DateTime.Now;
                }


                //DepoStok_Table'da güncelle
                _unitOfWork.DepoStok.Update(girisDepoStok);
            }
        }
        private bool DepoHareketeCikisKaydiGir(DepoHareketVM obj)
        {
            bool isSuccess = false;
            if (ModelState.IsValid)
            {
                if (obj.DepoHareket.Id == 0)
                {
                    string? userName = HttpContext.User.Identity.Name;
                    obj.DepoHareket.Olusturan = userName;
                    obj.DepoHareket.GirisCikis = ProjectConstants.MTS_CIKIS;
                    obj.DepoHareket.KaynakDepoId = obj.DepoHareket.KaynakDepoId;
                    obj.DepoHareket.HedefDepoId = obj.DepoHareket.HedefDepoId;

                    _unitOfWork.DepoHareket.Add(obj.DepoHareket);
                    TempData["success"] = "Depo işlemi gerçekleşti";
                    isSuccess = true;
                }
                else
                {
                    TempData["error"] = "Depo Id bulunamadı";
                }

                //_unitOfWork.Save();


            }
            return isSuccess;
        }

        public JsonResult GetDepoListExceptThis(int depoId)
        {
            List<DepoTanim> depoTanims = _unitOfWork.DepoTanim.GetAll().Where(u => u.Id != depoId).ToList();
            return Json(depoTanims);
        }


        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var depoHareketList = _unitOfWork.DepoHareket.GetAll(includeProperties: "DepoTanim,AniObjesiTanim");
            return Json(new { data = depoHareketList });
        }
        public IActionResult GetTransfers()
        {
            var depoHareketList = _unitOfWork.DepoHareket.GetByFilter(u => u.KaynakId == 0, includeProperties: "DepoTanim,KaynakDepoTanim,AniObjesiTanim");
            return Json(new { data = depoHareketList });
        }

        //Delete
        [HttpPost]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _unitOfWork.DepoHareket.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Kayıt silmede hata" });
            }

            _unitOfWork.DepoHareket.Remove(obj);
            DepoHareketVM depoHareketVM = new()
            {
                DepoHareket = obj

            };
            DepoStoktanCikisAdediDus(depoHareketVM);

            string? userName = HttpContext.User.Identity.Name;
            obj.Olusturan = userName;
            _unitOfWork.Save();
            return Json(new { success = true, message = "Depo işlemi silindi" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(DepoHareketVM obj)
        {
            if (ModelState.IsValid)
            {
                DepoStokaGirisAdediEkle(obj);
                if (obj.DepoHareket.Id == 0)
                {
                    string? userName = HttpContext.User.Identity.Name;
                    obj.DepoHareket.Olusturan = userName;
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
                DepoStoktanCikisAdediDus(obj);
                DepoStokaGirisAdediEkle(obj);
                if (obj.DepoHareket.Id == 0)
                {
                    TempData["error"] = "Depo Id bulunamadı";
                }
                else
                {

                    string? userName = HttpContext.User.Identity.Name;
                    obj.DepoHareket.Degistiren = userName;
                    _unitOfWork.DepoHareket.Update(obj.DepoHareket);
                    TempData["success"] = "Depo işlemi güncellendi";
                }
                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Transfer(DepoHareketVM obj)
        {
            //Buraya 2 tane DepoHareketVM gelmeli ya da bişey

            if (ModelState.IsValid)
            {

                DepoStoktanCikisAdediDus(obj);
                DepoStokaGirisAdediEkle(obj);
                DepoHareketeCikisKaydiGir(obj);
                //bool depoHareketeGirisKaydiGirildi = DepoHareketeGirisKaydiGir(obj);
                //if (obj.DepoHareket.Id == 0)
                //{
                //    _unitOfWork.DepoHareket.Add(obj.DepoHareket);
                //    TempData["success"] = "Depo işlemi gerçekleşti";
                //}
                //else
                //{
                //    TempData["error"] = "Depo Id bulunamadı";
                //}

                _unitOfWork.Save();

                return RedirectToAction("TransferIndex");
            }
            return View(obj);
        }
        #endregion

    }


}
