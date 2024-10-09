using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.DYS;
using Jenga.Models.MTS;
using Jenga.Utility;
using Jenga.Web.Areas.Admin.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class MalzemeCinsiController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public MalzemeCinsiController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
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
            SelectListItem bosItem = new SelectListItem
            {
                Text = "",
                Value = "0",
            };
            MalzemeCinsiVM malzemeCinsiVM = new()
            {

                MalzemeCinsi = new(),

                MalzemeGrubuSelectList = _unitOfWork.MalzemeGrubu.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
                UstMalzemeSelectList = _unitOfWork.MalzemeCinsi.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }).Append(bosItem),
            };


            return View(malzemeCinsiVM);

        }
        public IActionResult Edit(int? id)
        {
            SelectListItem bosItem = new SelectListItem
            {
                Text = "",
                Value = "0",
            };
            MalzemeCinsiVM malzemeCinsiVM = new()
            {
                MalzemeCinsi = new(),

                MalzemeGrubuSelectList = _unitOfWork.MalzemeGrubu.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
                UstMalzemeSelectList = _unitOfWork.MalzemeCinsi.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }).Append(bosItem),

            };

            if (id == null || id == 0)
            {
                return NotFound();
            }
            else
            {
                //update 
                malzemeCinsiVM.MalzemeCinsi = _unitOfWork.MalzemeCinsi.GetFirstOrDefault(u => u.Id == id);
                return View(malzemeCinsiVM);
            }

        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var malzemeCinsiList = _unitOfWork.MalzemeCinsi.GetAll(includeProperties: "MalzemeGrubu");
            return Json(new { data = malzemeCinsiList });
        }
        [HttpGet]
        public List<MalzemeCinsiVM> GetAllMalzemeCinsleri()
        {


            List<MalzemeCinsi> malzemeCinsleri = _unitOfWork.MalzemeCinsi.GetByFilter(x => x.Id > 0, includeProperties: "MalzemeGrubu").ToList();
            List<MalzemeCinsiVM> malzemeCinsiVMs = new List<MalzemeCinsiVM>();

            foreach (var malzemeCinsi in malzemeCinsleri)
            {
                MalzemeCinsiVM malzemeCinsiVM = new MalzemeCinsiVM
                {
                    MalzemeCinsi = malzemeCinsi,
                    UstMalzemeCinsi = _unitOfWork.MalzemeCinsi.GetFirstOrDefault(u => u.Id == malzemeCinsi.UstMalzemeCinsiId),
                };

                malzemeCinsiVMs.Add(malzemeCinsiVM);
            }

            return malzemeCinsiVMs;
        }
        private List<MalzemeCinsiVM> GetSubMalzeme(int parentId)
        {
            List<MalzemeCinsi> malzemeCinsleri = _unitOfWork.MalzemeCinsi.GetByFilter(u => u.UstMalzemeCinsiId == parentId);
            List<MalzemeCinsiVM> subMalzemeCinsleri = new List<MalzemeCinsiVM>();

            foreach (var malzemeCinsi in malzemeCinsleri)
            {
                if (malzemeCinsi.UstMalzemeCinsiId == parentId)
                {
                    MalzemeCinsiVM malzemeCinsiVM = new MalzemeCinsiVM
                    {
                        MalzemeCinsi = malzemeCinsi,
                        SubMalzeme = GetSubMalzeme(malzemeCinsi.Id)
                    };

                    subMalzemeCinsleri.Add(malzemeCinsiVM);
                }
            }

            return subMalzemeCinsleri;
        }
        [HttpGet]
        public IActionResult GetMalzemeTanimList()
        {
            var malzemeCinsiList = GetAllMalzemeCinsleri();
            var malzemeCinsleriSortedList = malzemeCinsiList.OrderBy(x => x.MalzemeCinsi.UstMalzemeCinsiId);

            return Json(new { data = malzemeCinsleriSortedList });
        }
        //Delete
        [HttpPost]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _unitOfWork.MalzemeCinsi.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new {success=false, message="Kayıt silmede hata"});
            }
            _unitOfWork.MalzemeCinsi.Remove(obj);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Malzeme Cinsi silindi" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MalzemeCinsiVM obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.MalzemeCinsi.Id == 0)
                {
                    string? userName = HttpContext.User.Identity.Name;
                    obj.MalzemeCinsi.Olusturan = userName;
                    _unitOfWork.MalzemeCinsi.Add(obj.MalzemeCinsi);
                    TempData["success"] = "Malzeme Cinsi oluşturuldu";
                }
                else
                {
                    TempData["error"] = "Malzeme Cinsi oluşturulamadı";
                }
                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(MalzemeCinsiVM obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.MalzemeCinsi.Id == 0)
                {
                    TempData["success"] = "Malzeme Cinsi Id bulunamadı";
                }
                else
                {
                    string? userName = HttpContext.User.Identity.Name;
                    obj.MalzemeCinsi.Degistiren = userName;
                    _unitOfWork.MalzemeCinsi.Update(obj.MalzemeCinsi);
                    TempData["success"] = "Malzeme Cinsi güncellendi";
                }
                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }
        #endregion
    }


}
