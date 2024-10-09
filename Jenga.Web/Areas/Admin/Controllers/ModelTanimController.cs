using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.DYS;
using Jenga.Models.MTS;
using Jenga.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class ModelTanimController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public ModelTanimController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
            _cache = cache;
        }
        private readonly IMemoryCache _cache;
        public IActionResult Index()
        {
            return View();
        }

        //GET
        public IActionResult Create()
        {

            ModelVM modelVM = new()
            {
                ModelTanim = new(),
               
                MarkaTanimList = _unitOfWork.MarkaTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),

            };

            return View(modelVM);

        }
        public IActionResult Edit(int? id)
        {
            ModelVM modelVM = new()
            {
                ModelTanim = new(),
                MarkaTanimList = _unitOfWork.MarkaTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),

            };

            if (id == null || id == 0)
            {
                return NotFound();
            }
            else
            {
                //update 
                modelVM.ModelTanim = _unitOfWork.ModelTanim.GetFirstOrDefault(u => u.Id == id);
                return View(modelVM);
            }

        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {


            var modelList = _unitOfWork.ModelTanim.GetAll(includeProperties: "MarkaTanim");
            return Json(new { data = modelList });
            //return Json(new { data = cachedObject });
        }
        private IEnumerable<ModelTanim> GetDataFromDataSource()
        {
            // Code to fetch the data from the data source
            // ...
            var modelList = _unitOfWork.ModelTanim.GetAll(includeProperties: "MarkaTanim");
            return  modelList;
        }
        //Delete
        [HttpPost]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _unitOfWork.ModelTanim.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new {success=false, message="Kayıt silmede hata"});
            }
            _unitOfWork.ModelTanim.Remove(obj);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Model silindi" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ModelVM obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.ModelTanim.Id == 0)
                {
                    string? userName = HttpContext.User.Identity.Name;
                    obj.ModelTanim.Olusturan = userName;
                    _unitOfWork.ModelTanim.Add(obj.ModelTanim);
                    TempData["success"] = "Model oluşturuldu";
                }
                else
                {
                    TempData["error"] = "Model oluşturulamadı";
                }
                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ModelVM obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.ModelTanim.Id == 0)
                {
                    TempData["success"] = "Model Id bulunamadı";
                }
                else
                {
                    string? userName = HttpContext.User.Identity.Name;
                    obj.ModelTanim.Degistiren = userName;
                    _unitOfWork.ModelTanim.Update(obj.ModelTanim);
                    TempData["success"] = "Model güncellendi";
                }
                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }
        #endregion
    }


}
