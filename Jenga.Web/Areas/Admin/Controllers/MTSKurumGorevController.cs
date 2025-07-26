using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.MTS;
using Jenga.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class MTSKurumGorevController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public MTSKurumGorevController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public JsonResult GetMTSGorevTanimList(int mtsKurumTanimId)
        {
            List<MTSGorevTanim> list = _unitOfWork.MTSGorevTanim.GetByFilter(u => u.MTSKurumTanimId == mtsKurumTanimId).ToList();
            return Json(list);
        }
        [HttpGet]
        public IActionResult Create(int kisiId)
        {
            Kisi kisi = _unitOfWork.Kisi.GetFirstOrDefault(u => u.Id == kisiId);

            var durumList = new List<SelectListItem> {
              new SelectListItem { Text = ProjectConstants.MTSGOREVDURUMU_GOREVDE, Value = ProjectConstants.MTSGOREVDURUMU_GOREVDE},
              new SelectListItem { Text = ProjectConstants.MTSGOREVDURUMU_AYRILDI, Value = ProjectConstants.MTSGOREVDURUMU_AYRILDI}
            };
            var mtsKurumIdList = _unitOfWork.MTSKurumGorev.GetByFilter(u => u.Durum == ProjectConstants.MTSGOREVDURUMU_GOREVDE).Select(k => k.MTSKurumTanimId).ToList();
            var mtsGorevIdList = _unitOfWork.MTSKurumGorev.GetByFilter(u => u.Durum == ProjectConstants.MTSGOREVDURUMU_GOREVDE).Select(k => k.MTSGorevTanimId).ToList();
            var mtsKurumTanimList = _unitOfWork.MTSKurumTanim.GetAll();
            var mtsGorevTanimList = _unitOfWork.MTSGorevTanim.GetAll();//.Where(u => (!mtsKurumIdList.Contains(u.Id) && !mtsGorevIdList.Contains(u.Id)));
            MTSKurumGorevVM mtsKurumGorevVM = new()
            {
                MTSKurumGorev = new(),

                DurumList = durumList,
                MTSKurumTanimList = mtsKurumTanimList.Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }).OrderBy(a => a.Text),
                MTSGorevTanimList = mtsGorevTanimList.Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }).OrderBy(a => a.Text),
            };
            mtsKurumGorevVM.Kisi = kisi;//ekranda kişi bilgilerini görmek için
            mtsKurumGorevVM.MTSKurumGorev.KisiId = kisiId;
            return View(mtsKurumGorevVM);
        }
        [HttpGet]
        public IActionResult Edit(int? id)
        {

            var mtsKurumGorevDb = _unitOfWork.MTSKurumGorev.GetFirstOrDefault(
                u => u.Id == id && !string.IsNullOrEmpty(u.Durum) &&
                u.Durum.Equals(ProjectConstants.MTSGOREVDURUMU_GOREVDE), includeProperties: "MTSKurumTanim,MTSGorevTanim,Kisi");

            var durumList = new List<SelectListItem> {
              new SelectListItem { Text = ProjectConstants.MTSGOREVDURUMU_GOREVDE, Value = ProjectConstants.MTSGOREVDURUMU_GOREVDE},
              new SelectListItem { Text = ProjectConstants.MTSGOREVDURUMU_AYRILDI, Value = ProjectConstants.MTSGOREVDURUMU_AYRILDI}
            };
            var mtsKurumIdList = _unitOfWork.MTSKurumGorev.GetByFilter(u => u.Durum == ProjectConstants.MTSGOREVDURUMU_GOREVDE).Select(k => k.MTSKurumTanimId).ToList();
            var mtsKurumTanimList = _unitOfWork.MTSKurumTanim.GetAll().Where(u => !mtsKurumIdList.Contains(u.Id));
            MTSKurumGorevVM mtsKurumGorevVM = new()
            {
                MTSKurumGorev = new(),

                DurumList = durumList,

            };

            if (mtsKurumGorevDb == null || mtsKurumGorevDb.Id == 0)
            {
                return NotFound();
            }
            else
            {
                //update 
                mtsKurumGorevVM.Kisi = mtsKurumGorevDb.Kisi;//ekranda kişi bilgilerini görmek için
                mtsKurumGorevVM.MTSKurumGorev = mtsKurumGorevDb;//_unitOfWork.MTSKurumGorev.GetFirstOrDefault(u => u.Id == mtsKurumGorevDb.Id, includeProperties: "MTSKurumTanim,MTSGorevTanim,Kisi");
                return View(mtsKurumGorevVM);
            }

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MTSKurumGorevVM obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.MTSKurumGorev.Id == 0)
                {
                    string? userName = HttpContext.User.Identity.Name;
                    obj.MTSKurumGorev.Olusturan = userName;
                    _unitOfWork.MTSKurumGorev.Add(obj.MTSKurumGorev);
                    TempData["success"] = "Görev oluşturuldu";
                }
                else
                {
                    TempData["error"] = "Görev oluşturulamadı";
                }

                _unitOfWork.Save();

                return RedirectToAction("Index", "Kisi");
            }
            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(MTSKurumGorevVM obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.MTSKurumGorev.Id == 0)
                {
                    TempData["success"] = "Görev bulunamadı";
                }
                else
                {
                    string? userName = HttpContext.User.Identity.Name;
                    obj.MTSKurumGorev.Degistiren = userName;
                    _unitOfWork.MTSKurumGorev.Update(obj.MTSKurumGorev);
                    TempData["success"] = "Görev güncellendi";
                }

                _unitOfWork.Save();

                return RedirectToAction("Index", "Kisi");
            }
            return View(obj);
        }


        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<MTSKurumGorev> objMTSKurumGorevListesi = _unitOfWork.MTSKurumGorev.GetAll(includeProperties: "MTSKurumTanim,MTSGorevTanim,Kisi").ToList();
            return Json(new { data = objMTSKurumGorevListesi });
        }
        [HttpGet]
        public IActionResult GetAllByMTSKurumTanimId(int mtsKurumTanimId)
        {
            List<MTSKurumGorev> list = _unitOfWork.MTSKurumGorev.GetByFilter(u => (u.MTSKurumTanimId == mtsKurumTanimId) &&
                (u.Durum.Equals(ProjectConstants.MTSGOREVDURUMU_GOREVDE)), includeProperties: "Kisi,MTSGorevTanim").ToList();
            var aa = JsonConvert.SerializeObject(list, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
            var result = new JsonResult(JsonConvert.DeserializeObject(aa));
            //return Json(new { data = list });
            return Json(new { data = result.Value });
        }
        [HttpPost]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _unitOfWork.MTSKurumGorev.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Görev silinemedi" });
            }
            _unitOfWork.MTSKurumGorev.Remove(obj);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Görev silindi" });

        }

        #endregion

    }
}
