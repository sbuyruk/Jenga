using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.DYS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Jenga.Web.Areas.Admin.Controllers
{
    //[Area("Admin")]
    public class MalzemeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        // GET: Admin/Malzeme

        public IActionResult Index()
        {

            return View();
        }
        public MalzemeController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }
        #region CreateEditDelete


        // GET: Admin/Malzeme/Create
        public IActionResult Create()
        {

            MalzemeVM malzemeVM = new()
            {
                Malzeme = new(),

                MalzemeCinsiList = _unitOfWork.MalzemeCinsi.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
                MarkaTanimList = _unitOfWork.MarkaTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
                ModelTanimList = _unitOfWork.ModelTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
            };

            return View(malzemeVM);

        }

        // POST: Admin/Malzeme/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MalzemeVM obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.Malzeme.Id == 0)
                {
                    string? userName = HttpContext.User.Identity.Name;
                    obj.Malzeme.Olusturan = userName;
                    _unitOfWork.Malzeme.Add(obj.Malzeme);
                    TempData["success"] = "Malzeme oluşturuldu";
                }
                else
                {
                    TempData["error"] = "Malzeme oluşturulamadı";
                }
                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }

        // GET: Admin/Malzeme/Edit/5
        public IActionResult Edit(int? id)
        {
            MalzemeVM malzemeVM = new()
            {
                Malzeme = new(),

                MalzemeCinsiList = _unitOfWork.MalzemeCinsi.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
                MarkaTanimList = _unitOfWork.MarkaTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
                ModelTanimList = _unitOfWork.ModelTanim.GetAll().Select(i => new SelectListItem
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
                malzemeVM.Malzeme = _unitOfWork.Malzeme.GetFirstOrDefault(u => u.Id == id);
                return View(malzemeVM);
            }

        }

        // POST: Admin/Malzeme/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(MalzemeVM obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.Malzeme.Id == 0)
                {
                    TempData["success"] = "Malzeme Id bulunamadı";
                }
                else
                {
                    string? userName = HttpContext.User.Identity.Name;
                    obj.Malzeme.Degistiren = userName;
                    _unitOfWork.Malzeme.Update(obj.Malzeme);
                    TempData["success"] = "Malzeme güncellendi";
                }
                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }
        //Delete
        [HttpPost]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _unitOfWork.Ozellik.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Kayıt silmede hata" });
            }
            _unitOfWork.Ozellik.Remove(obj);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Malzeme özelliği silindi" });
        }
        #endregion
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var malzemeList = _unitOfWork.Malzeme.GetAll(includeProperties: "MalzemeCinsi,MarkaTanim,ModelTanim");
            return Json(new { data = malzemeList });
            //return Json(new { data = cachedObject });
        }

        private IEnumerable<Ozellik> GetDataFromDataSource()
        {
            // Code to fetch the data from the data source
            // ...
            var malzemeCinsiList = _unitOfWork.Ozellik.GetAll(includeProperties: "MalzemeCinsi");
            return malzemeCinsiList;
        }

        #endregion

    }
}
