using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models;
using Jenga.Models.DYS;
using Jenga.Models.MTS;
using Jenga.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class MalzemeDagilimController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public MalzemeDagilimController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Create()
        {
            // Fetch the list of available Malzeme items from the database
            var malzemeList = _unitOfWork.Malzeme.GetAll()
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Adi // Adjust according to your Malzeme model's properties
                }).ToList();

            // Fetch the list of available MalzemeYeri 
            var malzemeYeriList = _unitOfWork.MalzemeYeriTanim.GetAll()
                .Select(y => new SelectListItem
                {
                    Value = y.Id.ToString(),
                    Text = y.Adi // Adjust according to your MalzemeYeri model's properties
                }).ToList();
            var islemList = new List<SelectListItem> {
              new SelectListItem { Text = "Sayım", Value = "Sayım" },
              new SelectListItem { Text = "Satınalma", Value = "Satınalma" },
              new SelectListItem { Text = "Sayım Düzeltme", Value = "Sayım Düzeltme" },
              
            };
            var viewModel = new MalzemeDagilimVM
            {
                MalzemeDagilim = new MalzemeDagilim { },
                MalzemeList = malzemeList,
                MalzemeYeriList = malzemeYeriList, 
                IslemList= islemList,
            };

            return View(viewModel);
        }
        [HttpPost]
        public IActionResult Create(MalzemeDagilimVM obj)
        {

            if (ModelState.IsValid)
            {
                string? userName = HttpContext.User.Identity.Name;
                //MalzemeHarekete kaydet
                MalzemeHareket mh = new MalzemeHareket
                {
                    MalzemeId=obj.MalzemeDagilim.MalzemeId,
                    KaynakYeriId= ProjectConstants.MALZEMENINGELDIGI_YER_BOS_INT,
                    HedefYeriId=obj.MalzemeDagilim.MalzemeYeriTanimId,
                    Adet=obj.MalzemeDagilim.Adet,
                    IslemTarihi=DateTime.Now,
                    IslemTipi=obj.Islem,
                    Aciklama=obj.MalzemeDagilim.Aciklama,
                    GirisCikis=ProjectConstants.MALZEMEHAREKETI_GIRIS,
                    Olusturan = userName,
                };
                _unitOfWork.MalzemeHareket.Add(mh);
                //MalzemeDagilimda bu malzeme bu malzemeyerinde var mı bak
                //varsa update et
                //yoksa insert et

                var entity = _unitOfWork.MalzemeDagilim.GetFirstOrDefault(m =>m.MalzemeId== obj.MalzemeDagilim.MalzemeId 
                    && m.MalzemeYeriTanimId==obj.MalzemeDagilim.MalzemeYeriTanimId);

                if (entity != null)
                {
                    // Modify the entity's properties
                    entity.Adet += obj.MalzemeDagilim.Adet;
                    entity.Aciklama =obj.MalzemeDagilim.Aciklama;
                    obj.MalzemeDagilim.Degistiren = userName;
                    // Update entity
                    _unitOfWork.MalzemeDagilim.Update(entity);
                }
                else
                {
                    obj.MalzemeDagilim.Olusturan = userName;
                    _unitOfWork.MalzemeDagilim.Add(obj.MalzemeDagilim);

                }
                TempData["success"] = "Kaydedildi";
                _unitOfWork.Save();// Commit the changes

                return RedirectToAction("Index");
            }
            TempData["error"] = "Kaydedilemedi";
            return View(obj);
        }
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            //var malzemeDagilimList = _unitOfWork.MalzemeDagilim.GetAll(includeProperties:"Malzeme,MalzemeYeriTanim,MalzemeCinsi");
            var malzemeDagilimList = _unitOfWork.MalzemeDagilim.GetMalzemeDagilimWithDetails();

            return Json(new { data = malzemeDagilimList });
        }
        public JsonResult GetListByYer(int malzemeYeriTanimId)
        {
            List<MalzemeDagilim> malzemeDagilimList = _unitOfWork.MalzemeDagilim.GetByFilter(u => u.MalzemeYeriTanimId == malzemeYeriTanimId, includeProperties: "Malzeme,MalzemeYeriTanim").ToList();
            return Json(malzemeDagilimList);
        }
        #endregion
    }


}
