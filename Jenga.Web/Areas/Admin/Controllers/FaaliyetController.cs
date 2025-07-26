using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.MTS;
using Jenga.Utility;
using Jenga.Web.Areas.Admin.Services;
using Microsoft.AspNetCore.Mvc;


namespace Jenga.Web.Areas.Admin.Controllers
{
    public class FaaliyetController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly KatilimciService _katilimciService;
        private readonly FaaliyetService _faaliyetService;
        private readonly ToplantiService _toplantiService;

        public FaaliyetController(IUnitOfWork unitOfWork, KatilimciService katilimciService, FaaliyetService faaliyetService, ToplantiService toplantiService)
        {
            _unitOfWork = unitOfWork;
            _katilimciService = katilimciService;
            _faaliyetService = faaliyetService;
            _toplantiService = toplantiService;
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
        public IActionResult GetEvents(bool resmiTatiller, bool vakifToplantilari)
        {
            var form = HttpContext.Request.Form;
            var startDate = DateTime.Parse(form["start"]);
            var endDate = DateTime.Parse(form["end"]);
            List<CalendarEvent> Events = new List<CalendarEvent>();
            _faaliyetService.GetFaaliyetList(startDate, endDate, Events);
            if (resmiTatiller)
                _faaliyetService.GetResmiTatilList(startDate, endDate, Events);
            if (vakifToplantilari)
                _toplantiService.GetVakifToplantiList(startDate, endDate, Events);

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

        public IActionResult GetFaaliyetList(bool acikTarihliChecked)
        {
            var faaliyetList = acikTarihliChecked ? _unitOfWork.Faaliyet.GetAll() : _unitOfWork.Faaliyet.GetByFilter(a => a.AcikTarih != true);

            var faaliyetKatilimList = _unitOfWork.FaaliyetKatilim.GetAll(includeProperties: "Kisi");
            var list1 = from faaliyet in faaliyetList
                        join faaliyetKatilim in faaliyetKatilimList
                        on faaliyet.Id equals faaliyetKatilim.FaaliyetId into childGroup
                        from child in childGroup.DefaultIfEmpty()
                        select new { faaliyet };

            var result = list1
                      .Select(f => new
                      {
                          Faaliyet = f.faaliyet,
                          Kisiler = f.faaliyet.FaaliyetKatilims == null ? null :
                          (
                            string.Join("<br> ", f.faaliyet.FaaliyetKatilims.Select(t => t.Kisi).Select(k => k.Adi + " " + k.Soyadi))
                            )

                      }).GroupBy(x => x.Faaliyet.Id).Select(x => x.First()).ToList(); //remove dublicates SB

            return Json(new { data = result });
        }
        public IActionResult GetAllAcikTarihliFaaliyetList(bool toplantiChecked, bool ziyaretChecked, bool gorusmeChecked, bool seyahatChecked, bool davetChecked)
        {

            List<Faaliyet> faaliyetList = new List<Faaliyet>();// _unitOfWork.Faaliyet.GetAll(includeProperties:"FaaliyetAmaci").Where(u => u.AcikTarih == true).ToList();
            if (toplantiChecked)
            {
                int x = 0;

                Int32.TryParse(ProjectConstants.FAALIYET_AMACI_TOPLANTI_INT, out x);
                var list = _unitOfWork.Faaliyet.GetAll(includeProperties: "FaaliyetAmaci").Where(u => u.AcikTarih == true && u.FaaliyetAmaciId == x).ToList();
                faaliyetList.AddRange(list);
            }
            if (ziyaretChecked)
            {
                int x = 0;

                Int32.TryParse(ProjectConstants.FAALIYET_AMACI_ZIYARET_INT, out x);
                var list = _unitOfWork.Faaliyet.GetAll(includeProperties: "FaaliyetAmaci").Where(u => u.AcikTarih == true && u.FaaliyetAmaciId == x).ToList();
                faaliyetList.AddRange(list);
            }
            if (gorusmeChecked)
            {
                int x = 0;

                Int32.TryParse(ProjectConstants.FAALIYET_AMACI_GORUSME_INT, out x);
                var list = _unitOfWork.Faaliyet.GetAll(includeProperties: "FaaliyetAmaci").Where(u => u.AcikTarih == true && u.FaaliyetAmaciId == x).ToList();
                faaliyetList.AddRange(list);
            }
            if (seyahatChecked)
            {
                int x = 0;

                Int32.TryParse(ProjectConstants.FAALIYET_AMACI_SEYAHAT_INT, out x);
                var list = _unitOfWork.Faaliyet.GetAll(includeProperties: "FaaliyetAmaci").Where(u => u.AcikTarih == true && u.FaaliyetAmaciId == x).ToList();
                faaliyetList.AddRange(list);
            }
            if (davetChecked)
            {
                int x = 0;

                Int32.TryParse(ProjectConstants.FAALIYET_AMACI_DAVET_INT, out x);
                var list = _unitOfWork.Faaliyet.GetAll(includeProperties: "FaaliyetAmaci").Where(u => u.AcikTarih == true && u.FaaliyetAmaciId == x).ToList();
                faaliyetList.AddRange(list);
            }

            var faaliyetKatilimList = _unitOfWork.FaaliyetKatilim.GetAll(includeProperties: "Kisi");
            var list1 = from faaliyet in faaliyetList
                        join faaliyetKatilim in faaliyetKatilimList
                        on faaliyet.Id equals faaliyetKatilim.FaaliyetId into childGroup
                        from child in childGroup.DefaultIfEmpty()
                        select new { faaliyet };
            var result = list1
                      .Select(f => new
                      {
                          Faaliyet = f.faaliyet,
                          Kisiler = f.faaliyet.FaaliyetKatilims == null ? null :
                          (
                            string.Join("<br> ", f.faaliyet.FaaliyetKatilims.Select(t => t.Kisi).Select(k => k.Adi + " " + k.Soyadi))
                            )

                      }).GroupBy(x => x.Faaliyet.Id).Select(x => x.First()).ToList(); //remove dublicates SB


            return Json(new { data = result });
        }

        #endregion
    }
}
