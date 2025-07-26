using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.DYS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class MalzemeHareketController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public MalzemeHareketController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }
        #region create
        public IActionResult Create()
        {
            // Fetch the list of available Malzeme items from the database
            var malzemeList = _unitOfWork.Malzeme.GetAll()
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Adi // Adjust according to your Malzeme model's properties
                }).ToList();

            // Fetch the list of available MalzemeYeri for both KaynakYeri and HedefYeri
            var malzemeYeriList = _unitOfWork.MalzemeYeriTanim.GetAll()
                .Select(y => new SelectListItem
                {
                    Value = y.Id.ToString(),
                    Text = y.Adi // Adjust according to your MalzemeYeri model's properties
                }).ToList();
            var girisCikisList = new List<SelectListItem> {
              new SelectListItem { Text = "Giriş", Value = "Giriş" },
              //new SelectListItem { Text = "Çıkış", Value = "Çıkış" } Depoya sadece giriş olsun
            };
            var viewModel = new MalzemeHareketVM
            {
                MalzemeHareket = new MalzemeHareket
                {
                    IslemTarihi = DateTime.Now // Initialize with current date
                },
                MalzemeList = malzemeList,
                KaynakYeriList = malzemeYeriList, // Same list for both KaynakYeri and HedefYeri
                HedefYeriList = malzemeYeriList,
                GirisCikisList = girisCikisList,
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Create(MalzemeHareketVM obj)
        {

            if (ModelState.IsValid)
            {
                //if (obj.MalzemeHareket.Id == 0)
                //{
                string? userName = HttpContext.User.Identity.Name;
                obj.MalzemeHareket.Olusturan = userName;
                _unitOfWork.MalzemeHareket.Add(obj.MalzemeHareket);
                TempData["success"] = "Malzeme Kaydedildi";
                //}
                //else
                //{
                //    TempData["error"] = "Malzeme Kaydedilemedi";
                //}

                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }
        #endregion
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            //var malzemeHareketList = _unitOfWork.MalzemeHareket.GetAll(includeProperties:"Malzeme,MalzemeYeriTanim,MalzemeCinsi");
            var malzemeHareketList = _unitOfWork.MalzemeHareket.GetMalzemeHareketWithDetails();
            var aa = JsonConvert.SerializeObject(malzemeHareketList, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
            var result = new JsonResult(JsonConvert.DeserializeObject(aa));
            return Json(new { data = result.Value });
        }
        public JsonResult GetListByYer(int kaynakYeriId)
        {
            List<MalzemeHareket> malzemeHareketList = _unitOfWork.MalzemeHareket.GetByFilter(u => u.KaynakYeriId == kaynakYeriId, includeProperties: "Malzeme,MalzemeYeriTanim").ToList();
            return Json(malzemeHareketList);
        }
        #endregion
    }


}
