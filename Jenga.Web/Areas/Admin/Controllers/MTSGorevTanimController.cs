using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.MTS;
using Jenga.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class MTSGorevTanimController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public MTSGorevTanimController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var objMTSGorevTanimListesi = _unitOfWork.MTSGorevTanim.GetAll();
            return View(objMTSGorevTanimListesi);
        }

        public IActionResult Create()
        {
            MTSGorevTanimVM mTSGorevTanimVM = new()
            {
                MTSGorevTanim = new(),
                MTSKurumTanimList = _unitOfWork.MTSKurumTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
            };
            return View(mTSGorevTanimVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MTSGorevTanimVM obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.MTSGorevTanim.Id == 0)
                {
                    string? userName = HttpContext.User.Identity.Name;
                    obj.MTSGorevTanim.Olusturan = userName;
                    _unitOfWork.MTSGorevTanim.Add(obj.MTSGorevTanim);
                    TempData["success"] = "Görev oluşturuldu";
                }
                else
                {
                    TempData["error"] = "Görev oluşturulamadı";
                }

                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }

        public IActionResult Edit(int? id)
        {

            if (id == null || id == 0)
            {
                return NotFound();
            }
            else
            {
                MTSGorevTanimVM mTSGorevTanimVM = new()
                {
                    MTSKurumTanimList = _unitOfWork.MTSKurumTanim.GetAll().Select(i => new SelectListItem
                    {
                        Text = i.Adi,
                        Value = i.Id.ToString()
                    }),
                };
                mTSGorevTanimVM.MTSGorevTanim= _unitOfWork.MTSGorevTanim.GetFirstOrDefault(u => u.Id == id);
                return View(mTSGorevTanimVM);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(MTSGorevTanimVM obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.MTSGorevTanim.Id == 0)
                {
                    TempData["success"] = "Görev bulunamadı";
                }
                else
                {
                    string? userName = HttpContext.User.Identity.Name;
                    obj.MTSGorevTanim.Degistiren = userName;
                    _unitOfWork.MTSGorevTanim.Update(obj.MTSGorevTanim);
                    TempData["success"] = "Görev güncellendi";
                }

                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }


        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<MTSGorevTanim> objMTSGorevTanimListesi = _unitOfWork.MTSGorevTanim.GetAll(includeProperties:"MTSKurumTanim").ToList();
            return Json(new { data = objMTSGorevTanimListesi });
        }
        [HttpGet]
        public IActionResult GetAllByMTSKurumTanimId(int mtsKurumTanimId)
        {
            List<MTSGorevTanim> list = _unitOfWork.MTSGorevTanim.GetByFilter(u => u.MTSKurumTanimId == mtsKurumTanimId, includeProperties: "MTSKurumTanim").ToList();
            var aa = JsonConvert.SerializeObject(list, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
            var result = new JsonResult(JsonConvert.DeserializeObject(aa));
            return Json(new { data = list });
        }
        [HttpPost]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _unitOfWork.MTSGorevTanim.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Görev silinemedi" });
            }
            _unitOfWork.MTSGorevTanim.Remove(obj);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Görev silindi" });

        }

        #endregion

    }
}
