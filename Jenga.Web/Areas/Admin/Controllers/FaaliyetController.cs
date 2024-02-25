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
    public class FaaliyetController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly KatilimciService _katilimciService;

        public FaaliyetController(IUnitOfWork unitOfWork, KatilimciService katilimciService)
        {
            _unitOfWork = unitOfWork;
            _katilimciService = katilimciService;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Faaliyet? faaliyetFromDb = _unitOfWork.Faaliyet.GetFirstOrDefault(u => u.Id == id);
 
            if (faaliyetFromDb == null)
            {
                return NotFound();
            }
            return View(faaliyetFromDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Faaliyet obj)
        {
            if (ModelState.IsValid)
            {
                string? userName = HttpContext.User.Identity.Name;
                obj.Olusturan = userName;
                _unitOfWork.Faaliyet.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Faaliyet  başarıyla oluşturuldu.";
                return RedirectToAction("Index");
            }
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Faaliyet obj)
        {
            if (ModelState.IsValid)
            {
                string? userName = HttpContext.User.Identity.Name;
                obj.Degistiren = userName;
                _unitOfWork.Faaliyet.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Faaliyet başarıyla güncellendi.";
                return RedirectToAction("Index");
            }
            return View();
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Faaliyet> list = _unitOfWork.Faaliyet.GetAll().ToList();
            return Json(new { data = list });
        }

        [HttpPost]
        public IActionResult Delete(int? id)
        {
            var faaliyetToBeDeleted = _unitOfWork.Faaliyet.GetFirstOrDefault(u => u.Id == id);
            
            if (faaliyetToBeDeleted == null)
            {
                return Json(new { success = false, message = "Faaliyet  Bulunamadı." });
            }
            else
            {
                _unitOfWork.Faaliyet.Remove(faaliyetToBeDeleted);
                _unitOfWork.Save();

                return Json(new { success = true, message = "Faaliyet  başarıyla silindi." }); 
            }

        }

        public IActionResult GetAllFormatted()
        {
            var objFaaliyetList = _unitOfWork.Faaliyet.IncludeIt(DateTime.Today.AddMonths(-3));
            foreach (var item in objFaaliyetList)
            {
                _katilimciService.FillKatilimciIntoFaaliyetKatilim(item.FaaliyetKatilims);
            }
            var aa = JsonConvert.SerializeObject(objFaaliyetList, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
            var result = new JsonResult(JsonConvert.DeserializeObject(aa));

            return Json(new { data = result.Value });
        }
        
        public IActionResult GetAllFaaliyetsWithKatilimci(DateTime? baslangicTarihi)
        {
            if (baslangicTarihi==null|| baslangicTarihi < ProjectConstants.ILK_TARIH)
            {
                baslangicTarihi = DateTime.Today.AddMonths(-3);
            }
            var faaliyet = _unitOfWork.Faaliyet.IncludeIt(baslangicTarihi);
            var katilimciList = _katilimciService.GetAllFaaliyetWithKatilimci(faaliyet.ToList());

            //var faaliyetKatilimList = faaliyet
            //    .GroupJoin(katilimciList,
            //        f => f.Id,
            //        fk => fk.FaaliyetId,
            //        (f, fk) => new
            //        {
            //            Faaliyet = f,
            //            //Katilimci = fk.Select(fk => fk.Katilimci),
            //            Katilimcilar = string.Join(", ", fk.Select(t => t.Adi + t.Soyadi))
            //        });
            var faaliyetKatilimList = faaliyet
                      .Select(f => new
                      {
                          Faaliyet = f,
                          Katilimcilar = f.FaaliyetKatilims == null ? null :
                          (
                            string.Join("<br> ", f.FaaliyetKatilims.Select(t => t.Katilimci).Select(k => k.Adi +" "+ k.Soyadi +" ("+k.Kurumu+ " - "+k.Gorevi+")"))
                            )

                      });

            return Json(new { data = faaliyetKatilimList });
        }
        
        #endregion

    }
}
