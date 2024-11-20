using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.DYS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class ZimmetController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public ZimmetController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        #region GET Index Create Edit Delete

        public IActionResult Index()
        {
            var zimmetList = _unitOfWork.Zimmet.GetAll(includeProperties: "Personel,Malzeme,MalzemeYeriTanim");
            return View(zimmetList);
        }
        public async Task<IActionResult> Create()
        {
            
            var personelList = _unitOfWork.Personel.GetByFilter(u => u.IsBilgileri.CalismaDurumu == "1", includeProperties: "Kimlik,IsBilgileri,PersonelMenu")
                .Select(y => new SelectListItem
                {
                    Value = y.Id.ToString(),
                    Text = y.Adi + " " + y.Soyadi
                }).ToList();
            var viewModel = new ZimmetVM
            {
                Zimmet = new Zimmet { Adet=1,Tarih=DateTime.Now},
                MalzemeList = await _unitOfWork.Malzeme.GetMalzemeDDL(true),
                MalzemeYeriList = await _unitOfWork.MalzemeYeriTanim.GetMalzemeYeriDDL(true), //malzemeYeriList,
                PersonelList = await _unitOfWork.Personel.GetPersonelDDL(true),
            };

            return View(viewModel);
        }
        //public IActionResult Create()
        //{
        //    ViewBag.PersonelList = new SelectList(_unitOfWork.Personel.GetAll(), "Id", "Adi");
        //    ViewBag.MalzemeList = new SelectList(_unitOfWork.Malzeme.GetAll(), "Id", "Adi");
        //    return View();
        //}
        // GET: Zimmet/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var zimmet = await _unitOfWork.Zimmet.GetFirstOrDefaultAsync(m=> m.Id==id, includeProperties:"Malzeme,Personel,MalzemeYeriTanim");
            if (zimmet == null)
                return NotFound();


            var viewModel = new ZimmetVM
            {
                Zimmet = zimmet,
                MalzemeList = await _unitOfWork.Malzeme.GetMalzemeDDL(true),
                MalzemeYeriList = await _unitOfWork.MalzemeYeriTanim.GetMalzemeYeriDDL(true), //malzemeYeriList,
                PersonelList = await _unitOfWork.Personel.GetPersonelDDL(true),
            };
            return View(viewModel);
        }

        #endregion

        #region POST Index Create Edit Delete
        // POST: Zimmet/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ZimmetVM zimmetVM)
        {
            if (ModelState.IsValid)
            {
                string? userName = HttpContext?.User?.Identity?.Name;
                zimmetVM.Zimmet.Olusturan = userName;
                await _unitOfWork.Zimmet.AddAsync(zimmetVM.Zimmet);
                MalzemeDagilim? malzemeDagilim = await _unitOfWork.MalzemeDagilim.GetFirstOrDefaultAsync(a=>a.MalzemeId== zimmetVM.Zimmet.MalzemeId && a.MalzemeYeriTanimId== zimmetVM.Zimmet.MalzemeYeriTanimId);
                if (malzemeDagilim == null)
                {
                    TempData["error"] = "Zimmet işlemi yapılamadı. Seçilen yerde yeterli miktarda malzeme yok.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    
                    if (malzemeDagilim.Adet - zimmetVM.Zimmet.Adet >= 0)
                    {
                        malzemeDagilim.Adet -= zimmetVM.Zimmet.Adet;
                        malzemeDagilim.Degistiren = userName;
                        _unitOfWork.MalzemeDagilim.Update(malzemeDagilim);
                        MalzemeHareket malzemeHareket = new() 
                        {
                            Aciklama= "Personele Zimmet yapıldı. PersonelId="+ zimmetVM.Zimmet.PersonelId+ " Açıklama= " +zimmetVM.Zimmet.Aciklama,
                            Adet= zimmetVM.Zimmet.Adet,
                            GirisCikis="Çıkış",
                            HedefYeriId= zimmetVM.Zimmet.PersonelId,
                            IslemTipi="Zimmet",
                            IslemTarihi=DateTime.Now,
                            KaynakYeriId= zimmetVM.Zimmet.MalzemeYeriTanimId,
                            MalzemeId= zimmetVM.Zimmet.MalzemeId,
                            Olusturan= userName,
                            
                        };

                        _unitOfWork.MalzemeHareket.Add(malzemeHareket);
                        
                    }else
                    {
                        TempData["error"] = "Zimmet işlemi yapılamadı. Seçilen yerde yeterli miktarda malzeme yok.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                await _unitOfWork.CommitAsync();
                TempData["success"] = "Zimmet işlemi gerçekleşti";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["error"] = "Zimmet işlemi yapılamadı";
                var malzemeList = _unitOfWork.Malzeme.GetAll()
                                .Select(m => new SelectListItem
                                {
                                    Value = m.Id.ToString(),
                                    Text = m.Adi
                                }).ToList();

                // Fetch the list of available MalzemeYeri 
                var malzemeYeriList = _unitOfWork.MalzemeYeriTanim.GetAll()
                    .Select(y => new SelectListItem
                    {
                        Value = y.Id.ToString(),
                        Text = y.Adi
                    }).ToList();
                var personelList = _unitOfWork.Personel.GetByFilter(u => u.IsBilgileri.CalismaDurumu == "1", includeProperties: "Kimlik,IsBilgileri,PersonelMenu")
                    .Select(y => new SelectListItem
                    {
                        Value = y.Id.ToString(),
                        Text = y.Adi + " " + y.Soyadi
                    }).ToList();
                var viewModel = new ZimmetVM
                {
                    Zimmet = new Zimmet { Adet = 1, Tarih = DateTime.Now },
                    MalzemeList = malzemeList,
                    MalzemeYeriList = malzemeYeriList,
                    PersonelList = personelList,
                };
            }
            return View(zimmetVM);
        }
        // POST: Zimmet/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ZimmetVM zimmetVM)
        {
            //MalzemeDagilim'da mevcut malzemeId'nin adedini mevcut Çıkış Yerine ekle
            //MalzemeDagilim'da yeni adedi, yeni çıkış yerinden düş
            if (ModelState.IsValid)
            {
                Zimmet guncellenenZimmet = zimmetVM.Zimmet;
                //Kayıtlı Zimmeti bul
                Zimmet? oncekiZimmet = await _unitOfWork.Zimmet.GetFirstOrDefaultAsync(z=> z.Id==zimmetVM.Zimmet.Id, trackChanges: false);
                if (oncekiZimmet == null)
                {
                    TempData["error"] = "Zimmet işlemi yapılamadı";
                }
                else
                {
                    string? userName = HttpContext.User.Identity.Name;
                    zimmetVM.Zimmet.Degistiren = userName;
                    
                    MalzemeDagilim oncekiMalzemeDagilim = _unitOfWork.MalzemeDagilim.GetFirstOrDefault(d=>d.MalzemeId==oncekiZimmet.MalzemeId && d.MalzemeYeriTanimId==oncekiZimmet.MalzemeYeriTanimId); 
                    if (oncekiMalzemeDagilim == null)
                    {
                        TempData["error"] = "MalzemeDagilim'da kayyıt bulnamadı yapılamadı";
                    }
                    else
                    {
                        //MalzemeDagilim'da mevcut malzemeId'nin adedini mevcut Çıkış Yerine ekle
                        oncekiMalzemeDagilim.Adet += oncekiZimmet.Adet;
                        oncekiMalzemeDagilim.Degistiren = userName;
                        oncekiMalzemeDagilim.Aciklama = oncekiMalzemeDagilim.Aciklama?.Trim() + " (" + "Zimmet Güncellemesi yapıldı."+DateTime.Now;
                        _unitOfWork.MalzemeDagilim.Update(oncekiMalzemeDagilim);
                        //MalzemeDagilim'da yeni adedi, yeni çıkış yerinden düş
                        MalzemeDagilim guncellenenMalzemeDagilim = _unitOfWork.MalzemeDagilim.GetFirstOrDefault(d => d.MalzemeId == guncellenenZimmet.MalzemeId && d.MalzemeYeriTanimId == guncellenenZimmet.MalzemeYeriTanimId);
                        if (guncellenenMalzemeDagilim == null)
                        {
                            TempData["error"] = "Zimmet işlemi yapılamadı. Seçilen yerde yeterli miktarda malzeme yok.";
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {

                            if (guncellenenMalzemeDagilim.Adet - zimmetVM.Zimmet.Adet >= 0)
                            {
                                guncellenenMalzemeDagilim.Adet -= zimmetVM.Zimmet.Adet;
                                guncellenenMalzemeDagilim.Degistiren = userName;
                                _unitOfWork.MalzemeDagilim.Update(guncellenenMalzemeDagilim);
                                guncellenenZimmet.Degistiren = userName;
                                oncekiZimmet = guncellenenZimmet;
                                _unitOfWork.Zimmet.Update(oncekiZimmet);
                                MalzemeHareket malzemeHareket = new()
                                {
                                    Aciklama = "Geçerli kaydın malzeme dağılım bilgileri güncellendi. MalzemeDagilimId=" + zimmetVM.Zimmet.PersonelId + " Açıklama= " + zimmetVM.Zimmet.Aciklama,
                                    Adet = zimmetVM.Zimmet.Adet,
                                    GirisCikis = "Çıkış",
                                    HedefYeriId = zimmetVM.Zimmet.PersonelId,
                                    IslemTipi = "Zimmet",
                                    IslemTarihi = DateTime.Now,
                                    KaynakYeriId = zimmetVM.Zimmet.MalzemeYeriTanimId,
                                    MalzemeId = zimmetVM.Zimmet.MalzemeId,
                                    Olusturan = userName,

                                };

                                _unitOfWork.MalzemeHareket.Add(malzemeHareket);

                                //MalzemeDagilim'da yeni adedi, yeni çıkış yerinden düş
                                await _unitOfWork.CommitAsync();
                                TempData["success"] = "Zimmet işlemi güncellendi";
                                return RedirectToAction(nameof(Index));

                            }
                            else
                            {
                                TempData["error"] = "Zimmet işlemi yapılamadı. Seçilen yerde yeterli miktarda malzeme yok.";
                                return RedirectToAction(nameof(Index));
                            }
                        }

                      
                    }
                    
                }
            }
            else
            {
                TempData["error"] = "Zimmet işlemi yapılamadı";
            }
            return View(zimmetVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var entity = await _unitOfWork.Zimmet.GetFirstOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                {
                    return NotFound();
                }
                MalzemeDagilim? malzemeDagilim = await _unitOfWork.MalzemeDagilim.GetFirstOrDefaultAsync(
                    m => m.MalzemeId == entity.MalzemeId 
                    && m.MalzemeYeriTanimId==entity.MalzemeYeriTanimId);
                if (malzemeDagilim != null)
                {
                    malzemeDagilim.Adet += entity.Adet;
                    malzemeDagilim.Aciklama += entity.Aciklama;
                    string? userName = HttpContext?.User?.Identity?.Name;
                    
                    malzemeDagilim.Degistiren=userName;
                    _unitOfWork.MalzemeDagilim.Update(malzemeDagilim); ;
                }
                _unitOfWork.Zimmet.Remove(entity);
                await _unitOfWork.CommitAsync(); // Save changes to the database

                TempData["SuccessMessage"] = "Item deleted successfully.";
                return RedirectToAction("Index"); // Redirect to the list page after deletion
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error occurred while deleting the item.";
                // Log the exception (using a logging framework like Serilog, NLog, etc.)
                return RedirectToAction("Index");
            }
        }
        #endregion

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var malzemeDagilimList = _unitOfWork.MalzemeDagilim.GetMalzemeDagilimWithDetails();

            return Json(new { data = malzemeDagilimList });
        }
        public JsonResult GetListByYer(int malzemeYeriTanimId)
        {
            List<MalzemeDagilim> malzemeDagilimList = _unitOfWork.MalzemeDagilim.GetByFilter(u => u.MalzemeYeriTanimId == malzemeYeriTanimId, includeProperties: "Malzeme,MalzemeYeriTanim").ToList();
            return Json(malzemeDagilimList);
        }
        
        public async Task<JsonResult> GetPersonelByMalzeme(int malzemeId)
        {
            var personelList = _unitOfWork.Personel.GetByFilter(u => u.IsBilgileri.CalismaDurumu == "1", includeProperties: "Kimlik,IsBilgileri")
                .Select(p => new
                {
                    Id = p.Id,
                    Adi = p.Adi + " " + p.Soyadi,
                    AdetSum = _unitOfWork.Zimmet.GetByFilter(z => z.PersonelId == p.Id && z.MalzemeId == malzemeId)
                        .Sum(z => (int?)z.Adet) ?? 0 // If no records found, default to 0
                })
                .ToList();

            return Json(personelList);
        }
        
        #endregion
    }


}
