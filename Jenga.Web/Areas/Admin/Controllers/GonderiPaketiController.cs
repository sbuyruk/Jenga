using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.MTS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class GonderiPaketiController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public GonderiPaketiController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
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
        [HttpGet]
        public IActionResult Create()
        {
            var gonderiAraciList = new List<SelectListItem> {
              new SelectListItem { Text = "Kargo", Value = "Kargo" },
              new SelectListItem { Text = "Vakıf Aracı ile Ulaştırma", Value = "Vakıf Aracı" },
              new SelectListItem { Text = "Elden Teslim", Value = "Vakıf Aracı" } ,
              new SelectListItem { Text = "Gn.Md. Ziyaretinde Teslim", Value = "Gn.Md. Ziyareti" }
            };

            GonderiPaketiVM gonderiPaketiVM = new()
            {
                GonderiPaketi = new(),
                DagitimYeriList = _unitOfWork.DagitimYeriTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
                GondermeAraciList = gonderiAraciList,
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
            };

            return View(gonderiPaketiVM);

        }
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();

            }
            else
            {
                var gonderiAraciList = new List<SelectListItem> {
              new SelectListItem { Text = "Kargo", Value = "Kargo" },
              new SelectListItem { Text = "Vakıf Aracı", Value = "Vakıf Aracı" }
                };

                GonderiPaketiVM gonderiPaketiVM = new()
                {
                    GonderiPaketi = new(),
                    DagitimYeriList = _unitOfWork.DagitimYeriTanim.GetAll().Select(i => new SelectListItem
                    {
                        Text = i.Adi,
                        Value = i.Id.ToString()
                    }),
                    GondermeAraciList = gonderiAraciList,
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
                };
                gonderiPaketiVM.GonderiPaketi = _unitOfWork.GonderiPaketi.GetFirstOrDefault(u => u.Id == id);
                return View(gonderiPaketiVM);

            }

        }
        #endregion

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var gonderiPaketiList = _unitOfWork.GonderiPaketi.GetAll(includeProperties: "DagitimYeriTanim");
            return Json(new { data = gonderiPaketiList });
        }


        //Delete
        [HttpPost]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _unitOfWork.GonderiPaketi.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Kayıt silmede hata" });
            }

            string? userName = HttpContext.User.Identity.Name;
            obj.Degistiren = userName;
            _unitOfWork.GonderiPaketi.Remove(obj);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Gönderi paketi silindi" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(GonderiPaketiVM obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.GonderiPaketi.Id == 0)
                {
                    string? userName = HttpContext.User.Identity.Name;
                    obj.GonderiPaketi.Olusturan = userName;
                    _unitOfWork.GonderiPaketi.Add(obj.GonderiPaketi);
                    TempData["success"] = "Gönderi Paketi Tamamlandı";
                }
                else
                {
                    TempData["error"] = "Gönderi Paketi Id bulunamadı";
                }

                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(GonderiPaketiVM obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.GonderiPaketi.Id == 0)
                {
                    TempData["error"] = "Gönderi paketi Id bulunamadı";
                }
                else
                {

                    _unitOfWork.GonderiPaketi.Update(obj.GonderiPaketi);
                    TempData["success"] = "Gönderi paketi güncellendi";
                }

                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }

        #endregion

    }


}
