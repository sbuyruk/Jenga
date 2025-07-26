using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.MTS;
using Jenga.Utility;
using Jenga.Web.Areas.Admin.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;


namespace Jenga.Web.Areas.Admin.Controllers
{
    public class KisiController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly KatilimciService _katilimciService;
        public KisiController(IUnitOfWork unitOfWork, KatilimciService katilimciService)
        {
            _unitOfWork = unitOfWork;
            _katilimciService = katilimciService;
        }
        //GET
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

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
                includeProperties: "MTSKurumTanim,MTSGorevTanim");
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
                MTSKurumu = mtsKurumGorevDb != null ? mtsKurumGorevDb.MTSKurumTanim : null,
                MTSGorevi = mtsKurumGorevDb != null ? mtsKurumGorevDb.MTSGorevTanim : null,
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
                }).OrderBy(a => a.Text),

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
        [HttpGet]
        public IActionResult Acik(int? id)
        {
            return View();
        }
        [HttpGet]
        public IActionResult KisiBul()
        {
            return View();
        }
        [HttpGet]
        public IActionResult KisilereTopluTasima()
        {
            return View();
        }
        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(KisiVM obj)
        {


            if ((obj == null) || (obj.Kisi == null) || string.IsNullOrEmpty(obj.Kisi.Adi.Trim()))
            {
                ModelState.AddModelError("adi", "Lütfen Kisi Adını boş bırakmayınız");
            }
            if (ModelState.IsValid)
            {
                string? userName = HttpContext.User.Identity.Name;
                obj.Kisi.Olusturan = userName;
                _unitOfWork.Kisi.Add(obj.Kisi);
                _unitOfWork.Save();
                TempData["success"] = "Kişi kayıtlara eklendi.";
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
            var faaliyetKatilim = _unitOfWork.FaaliyetKatilim.GetByFilter(r => r.KatilimciId.Equals(obj.Id) && r.KatilimciTipi == ProjectConstants.FAALIYET_KATILIMCI_DIS_INT);
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
                    var kurum = _unitOfWork.MTSKurumGorev.GetByFilter(u => u.KisiId == obj.Id && u.Durum.Equals(ProjectConstants.MTSGOREVDURUMU_GOREVDE));
                    if (kurum.Count > 0)
                    {
                        return Json(new { success = false, message = "Kayıt silinemez, Kişinin kurum/görev bağlantısı bulunmaktadır." });
                    }
                    else
                    {
                        var aramagorusme = _unitOfWork.AramaGorusme.GetByFilter(u => u.ArayanId == obj.Id);
                        if (aramagorusme.Count > 0)
                        {
                            return Json(new { success = false, message = "Kayıt silinemez, Arama/Görüşme kaydı bulunmaktadır." });
                        }
                        else
                        {
                            var aniObjesiDagitim = _unitOfWork.AniObjesiDagitim.GetByFilter(u => u.KatilimciId == obj.Id && u.KatilimciTipi == ProjectConstants.FAALIYET_KATILIMCI_DIS_INT);
                            if (aniObjesiDagitim.Count > 0)
                            {
                                return Json(new { success = false, message = "Kayıt silinemez, Anı Objesi dağıtım kaydı bulunmaktadır." });
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
                }

            }

        }
        [HttpPost]

        public IActionResult KisiBul(int katilimciId, int katilimciTipi)
        {
            //Katilimciyi bul,
            //Kisi tablosunda var mı? 
            //Yoksa;
            //Begin transaction
            // Yeni kisi yarat, bilgilerini doldur kaydet, kisi_table.katilimciId=eskiId,KatilimciTpi=X olsun
            // FaaliyetKatilim_Table'da Bu katilimciya ait IDleri YeniKisiId olacak şekilde update et
            // AramaGorusme_Table'da Bu katilimciya ait IDleri YeniKisiId olacak şekilde update et
            // AniObjesiDagitim_Table'da Bu katilimciya ait IDleri YeniKisiId olacak şekilde update et
            //End transaction
            Kisi kisi = _unitOfWork.Kisi.GetFirstOrDefault(u => u.KatilimciId == katilimciId && u.KatilimciTipi == katilimciTipi);
            if (kisi == null)
            {
                Katilimci katilimci = _katilimciService.GetKatilimci(katilimciId, katilimciTipi);
                if (katilimci == null)
                {
                    ModelState.AddModelError("adi", "Katılımcı bulunamadı");
                }
                else
                {
                    if (ModelState.IsValid)
                    {
                        string? userName = HttpContext.User.Identity.Name;
                        Kisi yeniKisi = new Kisi()
                        {
                            Aciklama = katilimci.Aciklama,
                            Adi = katilimci.Adi,
                            Adres = katilimci.Adres,
                            Dahili1 = katilimci.Dahili1,
                            Dahili2 = katilimci.Dahili2,
                            Dahili3 = katilimci.Dahili3,
                            Degistiren = userName,
                            DegistirmeTarihi = DateTime.Now,
                            DogumTarihi = katilimci.DogumTarihi,
                            EPosta = katilimci.Eposta,
                            Ilcesi = katilimci.Ilcesi,
                            Ili = katilimci.Ili,
                            KatilimciId = katilimciId,
                            KatilimciTipi = katilimciTipi,
                            Kutlama = katilimci.Kutlama.Value,

                            MTSUnvanTanimId = _katilimciService.CreateUnvan(katilimciTipi),
                            Olusturan = userName,
                            OlusturmaTarihi = DateTime.Now,
                            RandevuKisiti = false,
                            Soyadi = katilimci.Soyadi,
                            TCKimlikNo = katilimci.TCKimlikNo,
                            TelAciklama1 = katilimci.TelAciklama1,
                            TelAciklama2 = katilimci.TelAciklama2,
                            TelAciklama3 = katilimci.TelAciklama3,
                            Telefon1 = katilimci.Telefon1,
                            Telefon2 = katilimci.Telefon2,
                            Telefon3 = katilimci.Telefon3,
                            Unvani = katilimci.Unvani,
                        };
                        _unitOfWork.Kisi.Add(yeniKisi);
                        _unitOfWork.Save();
                        List<MTSKurumGorev> mTSKurumGorevs = _katilimciService.CreateKurumGorev(yeniKisi);
                        yeniKisi.MTSKurumGorevs = mTSKurumGorevs;
                        _unitOfWork.Kisi.Update(yeniKisi);
                        _unitOfWork.Save();
                        TempData["success"] = "Kisi kayıtlara taşındı.";
                        string redirectUrl = "/Admin/Kisi/Edit?Id=" + yeniKisi.Id;
                        return Json(new { redirectUrl });
                    }
                    else
                    {
                        TempData["error"] = "Kisi taşınamadı.";
                    }

                }
            }
            return View();
        }
        //APIs
        [HttpPost]
        public IActionResult KisilereTopluTasima(int katilimciTipi)
        {

            List<Kisi> list = YeniKisiYarat(katilimciTipi);
            foreach (var item in list)
            {
                //AramaGorusmeKayitlariniGuncelle(item);
                FaaliyetKatilimKayitlariniGuncelle(item);
                AniObjesiDagitimKayitlariniGuncelle(item);
            }


            return View();
        }
        private void FaaliyetKatilimKayitlariniGuncelle(Kisi yeniKisi)
        {
            var faaliyetKatilimList = _unitOfWork.FaaliyetKatilim.GetByFilter(a => a.KatilimciTipi == yeniKisi.KatilimciTipi && a.KatilimciId == yeniKisi.KatilimciId);
            if (faaliyetKatilimList != null)
            {
                foreach (var item in faaliyetKatilimList)
                {
                    item.KatilimciId = yeniKisi.Id;
                    _unitOfWork.FaaliyetKatilim.Update(item);
                }
                _unitOfWork.Save();
            }

        }
        private void AniObjesiDagitimKayitlariniGuncelle(Kisi yeniKisi)
        {
            var aniObjesiDagitimList = _unitOfWork.AniObjesiDagitim.GetByFilter(a => a.KatilimciTipi == yeniKisi.KatilimciTipi && a.KatilimciId == yeniKisi.KatilimciId);
            if (aniObjesiDagitimList != null)
            {
                foreach (var item in aniObjesiDagitimList)
                {
                    item.KatilimciId = yeniKisi.Id;
                    _unitOfWork.AniObjesiDagitim.Update(item);
                }
                _unitOfWork.Save();
            }

        }
        public class YourResultDto
        {
            public int Id { get; set; }
            public string Adi { get; set; }
            public decimal? TotalBagisMiktari { get; set; }
            public DateTime? MaxBagisTarihi { get; set; }
        }
        private List<Kisi> YeniKisiYarat(int katilimciTipi)
        {
            //Katilimciyi bul,
            //Kisi tablosunda var mı? 
            //Yoksa;
            //
            // Yeni kisi yarat, bilgilerini doldur kaydet, kisi_table.katilimciId=eskiId,KatilimciTpi=X olsun
            // FaaliyetKatilim_Table'da Bu katilimciya ait IDleri YeniKisiId olacak şekilde update et
            // AramaGorusme_Table'da Bu katilimciya ait IDleri YeniKisiId olacak şekilde update et
            // AniObjesiDagitim_Table'da Bu katilimciya ait IDleri YeniKisiId olacak şekilde update et
            //
            List<Kisi> returnList = new List<Kisi>();
            var searchList = Enumerable.Empty<int>();
            if (katilimciTipi == ProjectConstants.FAALIYET_KATILIMCI_IC_INT)
            {
                searchList = _unitOfWork.Personel.GetAll().Select(u => u.Id);
            }
            else if (katilimciTipi == ProjectConstants.FAALIYET_KATILIMCI_NAKITBAGISCI_INT)
            {

                var ids = new List<int>
                { 27298,74187,102883,113146,113147,113316,120835}; //arama gorusme bacağı
                //{
                //     40110,  65943,  69656,  80026,  80522, 113381, 114595, 117064,
                //    122963, 124131, 124355, 125310, 125346, 125600, 132762, 
                //    133277, 133535, 134472, 134898, 137207, 138085, 148104
                //};
                var idList = Enumerable.Empty<int>();
                //var result = from a in _unitOfWork.NakitBagisci.GetByFilter(u=>u.Sag==true && u.TCKimlikNo!=null && u.TCKimlikNo>0)
                var result = from a in _unitOfWork.NakitBagisci.GetAll().Where(bagisci => ids.Contains(bagisci.Id)).ToList()
                             join b in _unitOfWork.NakitBagisHareket.GetAll() on a.Id equals b.BagisciId into gj
                             from subBagis in gj.DefaultIfEmpty()
                             where subBagis != null && subBagis.BagisTarihi > new DateTime(2005, 1, 1)
                             group subBagis by new { a.Id, a.Adi } into grouped
                             let totalBagisMiktari = grouped.Sum(g => g.BagisMiktari)
                             let maxBagisTarihi = grouped.Max(g => g.BagisTarihi)
                             where totalBagisMiktari > 1 && maxBagisTarihi > new DateTime(2005, 1, 1)
                             select new YourResultDto
                             {
                                 Id = grouped.Key.Id,
                                 Adi = grouped.Key.Adi,
                                 TotalBagisMiktari = totalBagisMiktari,
                                 MaxBagisTarihi = maxBagisTarihi
                             };

                var xx = result.ToList();
                searchList = xx.Select(u => u.Id);

            }
            else if (katilimciTipi == ProjectConstants.FAALIYET_KATILIMCI_TASINMAZBAGISCI_INT)
            {
                //searchList = _unitOfWork.TasinmazBagisci.GetByFilter(u => u.Sag_vefat == "Sağ" && u.TCKimlikNo != null && u.TCKimlikNo > 0).Select(u => u.Id);
                var ids = new List<int>
                {
                     292,304,306,1338,1349,
                    1351,1357,1358,1360,1370,1372
                };
                var idList = Enumerable.Empty<int>();
                //var result = from a in _unitOfWork.NakitBagisci.GetByFilter(u=>u.Sag==true && u.TCKimlikNo!=null && u.TCKimlikNo>0)
                var result = _unitOfWork.TasinmazBagisci.GetAll().Where(bagisci => ids.Contains(bagisci.Id)).ToList();


                var xx = result.ToList();
                searchList = xx.Select(u => u.Id);
            }


            foreach (var itemId in searchList)
            {
                //Kayıtlı ise ekleme!
                Kisi kisi = _unitOfWork.Kisi.GetFirstOrDefault(u => u.KatilimciId == itemId && u.KatilimciTipi == katilimciTipi);
                if (kisi == null)
                {
                    Katilimci katilimci = _katilimciService.GetKatilimci(itemId, katilimciTipi);
                    if (katilimci != null)
                    {
                        string? userName = HttpContext.User.Identity.Name;
                        Kisi yeniKisi = new Kisi()
                        {
                            Adi = katilimci.Adi,
                            Adres = katilimci.Adres,
                            Dahili1 = katilimci.Dahili1,
                            Dahili2 = katilimci.Dahili2,
                            Dahili3 = katilimci.Dahili3,
                            Degistiren = userName,
                            DegistirmeTarihi = DateTime.Now,
                            DogumTarihi = katilimci.DogumTarihi,
                            EPosta = katilimci.Eposta,
                            Ilcesi = katilimci.Ilcesi,
                            Ili = katilimci.Ili,
                            KatilimciId = itemId,
                            KatilimciTipi = katilimciTipi,
                            Kutlama = katilimci.Kutlama.Value,

                            MTSUnvanTanimId = _katilimciService.CreateUnvan(katilimciTipi),
                            Olusturan = userName,
                            OlusturmaTarihi = DateTime.Now,
                            RandevuKisiti = false,
                            Soyadi = katilimci.Soyadi,
                            TCKimlikNo = katilimci.TCKimlikNo,
                            TelAciklama1 = katilimci.TelAciklama1,
                            TelAciklama2 = katilimci.TelAciklama2,
                            TelAciklama3 = katilimci.TelAciklama3,
                            Telefon1 = katilimci.Telefon1,
                            Telefon2 = katilimci.Telefon2,
                            Telefon3 = katilimci.Telefon3,
                            Unvani = katilimci.Unvani,
                        };

                        _unitOfWork.Kisi.Add(yeniKisi);
                        _unitOfWork.Save();
                        List<MTSKurumGorev> mTSKurumGorevs = _katilimciService.CreateKurumGorev(yeniKisi);
                        yeniKisi.MTSKurumGorevs = mTSKurumGorevs;
                        _unitOfWork.Kisi.Update(yeniKisi);
                        _unitOfWork.Save();
                        returnList.Add(yeniKisi);
                    }
                }
            }

            return returnList;
        }

        public IActionResult GetKisiListByFilter(string text)
        {
            //var Kisiler = _unitOfWork.Kisi.IncludeIt().Where(a => 
            //    a.Adi.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) >= 0   
            //    || a.Soyadi.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) >= 0);

            //var vakifCalisanlari = _unitOfWork.Personel.GetByFilter(a => (a!=null && !string.IsNullOrEmpty(a.Adi) 
            //    && (a.Adi.ToLower().Contains(text.ToLower()) || a.Soyadi.ToLower().Contains(text.ToLower()))));

            string lowerText = text.Trim().ToLower();
            List<Katilimci> KatilimciPersonelList = _katilimciService.GetKatilimciList(ProjectConstants.FAALIYET_KATILIMCI_IC_INT);
            List<Katilimci> KatilimciKisiList = _katilimciService.GetKatilimciList(ProjectConstants.FAALIYET_KATILIMCI_DIS_INT);

            List<Katilimci> KatilimciTasinmazBagisciList = _katilimciService.GetKatilimciList(ProjectConstants.FAALIYET_KATILIMCI_TASINMAZBAGISCI_INT);
            List<Katilimci> KatilimciNakitBagisciList = _katilimciService.GetKatilimciList(ProjectConstants.FAALIYET_KATILIMCI_NAKITBAGISCI_INT);

            var foundPersonel = KatilimciPersonelList.Where((a => (a != null && !string.IsNullOrEmpty(a.Adi) && (a.Adi.ToLower() + " " + a.Soyadi.ToLower()).Contains(lowerText))));
            var foundKisi = KatilimciKisiList.Where((a => (a != null && !string.IsNullOrEmpty(a.Adi) && (a.Adi.ToLower() + " " + a.Soyadi.ToLower()).Contains(lowerText))));
            var foundTasinmazBagisci = KatilimciTasinmazBagisciList.Where((a => (a != null && !string.IsNullOrEmpty(a.Adi) && (a.Adi.ToLower() + " " + a.Soyadi.ToLower()).Contains(lowerText))));
            var foundNakitBagisci = KatilimciNakitBagisciList.Where((a => (a != null && !string.IsNullOrEmpty(a.Adi) && (a.Adi.ToLower() + " " + a.Soyadi.ToLower()).Contains(lowerText))));

            var katilimcilar = new List<Katilimci>();
            //TcKimlikNo bazında unique olsun diye..., önce Kişi,personel,tasinmazBagisci,nakitBagisci sırası ile eklesin
            var mergedList = katilimcilar.Union(foundKisi, new KatilimciEqualityComparer())
                .Union(foundPersonel, new KatilimciEqualityComparer())
                .Union(foundTasinmazBagisci, new KatilimciEqualityComparer())
                .Union(foundNakitBagisci, new KatilimciEqualityComparer()).ToList();
            //bu alttaki yöntemde aynı isim birden fazla listede oluyor diye iptal edildi SB
            //katilimcilar.AddRange(foundPersonel);
            //katilimcilar.AddRange(foundKisi);
            //katilimcilar.AddRange(foundTasinmazBagisci);
            //katilimcilar.AddRange(foundNakitBagisci);


            var aa = JsonConvert.SerializeObject(mergedList, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
            var result = new JsonResult(JsonConvert.DeserializeObject(aa));

            return Json(new { data = result.Value });

        }
        [HttpGet]
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
                        if ((kurumGorev.Durum != null) && (kurumGorev.Durum.Equals(ProjectConstants.MTSGOREVDURUMU_GOREVDE)))
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
        public IActionResult GetKatilimci(int katilimciId, int katilimciTipi)
        {
            Katilimci katilimci = _katilimciService.GetKatilimci(katilimciId, katilimciTipi);
            return new JsonResult(katilimci);
        }
        private bool SilinebilirMi(Kisi obj)
        {
            bool silinebilirMi = true;
            //RandevuKatilim  kaydı varsa silinmesin          
            var faaliyetKatilim = _unitOfWork.FaaliyetKatilim.GetByFilter(r => r.KatilimciId.Equals(obj.Id) && r.KatilimciTipi == ProjectConstants.FAALIYET_KATILIMCI_DIS_INT);
            if (faaliyetKatilim.Count > 0)
            {
                silinebilirMi = false;
            }
            else
            {
                //RandevuKatilim  kaydı varsa silinmesin       
                var disIrtibat = _unitOfWork.Faaliyet.GetByFilter(u => u.DisIrtibatId == obj.Id);
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
