using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.IKYS;
using Jenga.Models.MTS;
using Jenga.Utility;
using Jenga.Web.Areas.Admin.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Drawing;

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
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]        
        public IActionResult Create()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Acik()
        {
            return View();
        }
        public IActionResult FaaliyetTakvimi()
        {
            return View();
        }
        [HttpPost]
        public IActionResult GetEvents()
        {
            var form= HttpContext.Request.Form;
            var startDate = DateTime.Parse(form["start"]);
            var endDate = DateTime.Parse(form["end"]);

            List<CalendarEvent> Events = new List<CalendarEvent>();
            var faaliyetList = _unitOfWork.Faaliyet.GetByFilter(a => startDate <= a.BaslangicTarihi && a.BitisTarihi <= endDate);
            foreach (var faaliyet in faaliyetList)
            {
                CalendarEvent anEvent = new CalendarEvent();
                anEvent.state = faaliyet.FaaliyetDurumu;

                anEvent.id = faaliyet.Id;
                int faaliyetAmaci = faaliyet.FaaliyetAmaci;
                anEvent.purpose = faaliyetAmaci.ToString();
                anEvent.title = faaliyet.FaaliyetKonusu;
                //item.description = item.title;
                anEvent.start = string.Format("{0:s}", faaliyet.BaslangicTarihi);
                anEvent.end = string.Format("{0:s}", faaliyet.BitisTarihi);
                anEvent.url = "http://tskgv-portal/Sayfalar/FaaliyetGirisi.aspx?DestinationApp=Duzenle&FaaliyetId=" + anEvent.id;
                anEvent.allDay = faaliyet.TumGun.Value;
                anEvent.startEditable = true;
                anEvent.description = faaliyet.Aciklama;
                RenkBelirle(anEvent);
                //item.className = "iptal-edildi";
                Events.Add(item: anEvent);
            }
            return Json(Events);
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
            var faaliyetler = _unitOfWork.Faaliyet.IncludeIt(baslangicTarihi);
            var katilimciList = _katilimciService.GetAllFaaliyetWithKatilimci(faaliyetler.ToList());

           
            var faaliyetKatilimList = faaliyetler
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
        public IActionResult GetAllAcikTarihliFaaliyetsWithKatilimci()
        {
            
            List<Faaliyet> faaliyetler = _unitOfWork.Faaliyet.IncludeIt(null).Where(u => u.AcikTarih == true).ToList();
            var katilimciList = _katilimciService.GetAllFaaliyetWithKatilimci(faaliyetler);

            var faaliyetKatilimList = faaliyetler
                      .Select(f => new
                      {
                          Faaliyet = f,
                          Katilimcilar = f.FaaliyetKatilims == null ? null :
                          (
                            string.Join("<br> ", f.FaaliyetKatilims.Select(t => t.Katilimci).Select(k => k.Adi + " " + k.Soyadi + " (" + k.Kurumu + " - " + k.Gorevi + ")"))
                            )

                      });

            return Json(new { data = faaliyetKatilimList });
        }
        public void RenkBelirle(CalendarEvent item)
        {

            switch (item.purpose)
            {
                case ProjectConstants.FAALIYET_AMACI_TOPLANTI_INT:
                    {
                        item.color = Color.Red.Name;
                        item.textColor = Color.White.Name;
                        item.purpose = ProjectConstants.FAALIYET_AMACI_TOPLANTI;
                        break;
                    }
                case ProjectConstants.FAALIYET_AMACI_ZIYARET_INT:
                    {
                        item.color = Color.Blue.Name;
                        item.textColor = Color.White.Name;
                        item.purpose = ProjectConstants.FAALIYET_AMACI_ZIYARET;
                        break;
                    }
                case ProjectConstants.FAALIYET_AMACI_DAVET_INT:
                    {
                        item.color = Color.Green.Name;
                        item.textColor = Color.White.Name;
                        break;
                    }
                case ProjectConstants.FAALIYET_AMACI_YILDONUMU_INT:
                    {
                        item.color = Color.Aqua.Name;
                        item.textColor = Color.Black.Name;
                        item.purpose = ProjectConstants.FAALIYET_AMACI_YILDONUMU;
                        break;
                    }
                case ProjectConstants.FAALIYET_AMACI_DOGUMGUNU_INT:
                    {
                        item.color = Color.Aquamarine.Name;
                        item.textColor = Color.Black.Name;
                        item.purpose = ProjectConstants.FAALIYET_AMACI_DOGUMGUNU;
                        break;
                    }
                case ProjectConstants.FAALIYET_AMACI_OZELCALISMA_INT:
                    {
                        item.color = Color.LightBlue.Name;
                        item.textColor = Color.White.Name;
                        item.purpose = ProjectConstants.FAALIYET_AMACI_OZELCALISMA;
                        break;
                    }
                case ProjectConstants.FAALIYET_AMACI_IZIN_INT:
                    {
                        item.color = Color.DarkViolet.Name;
                        item.textColor = Color.White.Name;
                        item.purpose = ProjectConstants.FAALIYET_AMACI_IZIN;
                        break;
                    }
                case ProjectConstants.FAALIYET_AMACI_RESMITATIL_INT:
                    {
                        item.color = Color.Purple.Name;
                        item.textColor = Color.White.Name;
                        item.purpose = ProjectConstants.FAALIYET_AMACI_RESMITATIL;
                        break;
                    }
                case ProjectConstants.FAALIYET_AMACI_GORUSME_INT:
                    {
                        item.color = Color.DeepSkyBlue.Name;
                        item.textColor = Color.White.Name;
                        item.purpose = ProjectConstants.FAALIYET_AMACI_GORUSME;
                        break;
                    }
                case ProjectConstants.FAALIYET_AMACI_SEYAHAT_INT:
                    {
                        item.color = Color.Coral.Name;
                        item.textColor = Color.White.Name;
                        item.purpose = ProjectConstants.FAALIYET_AMACI_SEYAHAT;
                        break;
                    }
                case ProjectConstants.FAALIYET_AMACI_BILGI_INT:
                    {
                        item.color = Color.DimGray.Name;
                        item.textColor = Color.White.Name;
                        item.purpose = ProjectConstants.FAALIYET_AMACI_BILGI;
                        break;
                    }
                default:
                    break;
            }
            if (item.state.Equals(ProjectConstants.FAALIYET_DURUMU_PLANLANDI_INT))
            {
                item.color = Color.LightGray.Name;
                item.textColor = Color.Black.Name;
            }

        }
        #endregion
    }
}
