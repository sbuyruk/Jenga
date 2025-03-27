using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.DYS;
using Jenga.Models.Ortak;
using Jenga.Models.TBYS;
using Jenga.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class TasinmazBagisciController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public TasinmazBagisciController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        #region GET Index Create Edit Delete

        public IActionResult Index()
        {
            var tasinmazBagisciList = _unitOfWork.TasinmazBagisci.GetAll();
            return View(tasinmazBagisciList);
        }
        public async Task<IActionResult> Create()
        {
            var sosyalGuvenceList = new List<SelectListItem> {
              new SelectListItem { Text = ProjectConstants.SOSYALGUVENCE_YOK, Value = ProjectConstants.SOSYALGUVENCE_YOK },
              new SelectListItem { Text = ProjectConstants.SOSYALGUVENCE_SGK, Value = ProjectConstants.SOSYALGUVENCE_SGK },
              new SelectListItem { Text = ProjectConstants.SOSYALGUVENCE_EMEKLISANDIGI, Value = ProjectConstants.SOSYALGUVENCE_EMEKLISANDIGI },
              new SelectListItem { Text = ProjectConstants.SOSYALGUVENCE_SSK, Value = ProjectConstants.SOSYALGUVENCE_SSK },
              new SelectListItem { Text = ProjectConstants.SOSYALGUVENCE_BAGKUR, Value = ProjectConstants.SOSYALGUVENCE_BAGKUR }
            };
            var sagVefatList = new List<SelectListItem> {
              new SelectListItem { Text = ProjectConstants.BAGISCI_SAG, Value = ProjectConstants.BAGISCI_SAG },
              new SelectListItem { Text = ProjectConstants.BAGISCI_VEFAT, Value = ProjectConstants.BAGISCI_VEFAT },
              new SelectListItem { Text = ProjectConstants.BAGISCI_BILINMIYOR, Value = ProjectConstants.BAGISCI_BILINMIYOR },
              new SelectListItem { Text = ProjectConstants.BAGISCI_MULGA, Value = ProjectConstants.BAGISCI_MULGA },
              new SelectListItem { Text = ProjectConstants.BAGISCI_KURULUS, Value = ProjectConstants.BAGISCI_KURULUS },
              new SelectListItem { Text = ProjectConstants.BAGISCI_TASFIYE, Value = ProjectConstants.BAGISCI_TASFIYE },
              new SelectListItem { Text = ProjectConstants.BAGISCI_LAGV, Value = ProjectConstants.BAGISCI_LAGV }
            };
            var viewModel = new TasinmazBagisciVM
            {
                TasinmazBagisci = new TasinmazBagisci { },
                SosyalGuvenceList = sosyalGuvenceList,
                SagVefatList = sagVefatList,
                
                IlVeIlceVM = new IlVeIlceVM
                {
                    Iller = (await _unitOfWork.Il.GetAllAsync())
                .Select(il => new SelectListItem { Value = il.Id.ToString(), Text = il.IlAdi })
                .ToList(),
                    Ilceler = new List<SelectListItem>(), // Başlangıçta boş
                    IlLabel = "İkamet İli",
                    IlceLabel = "İkamet İlçesi"
                }
            };

            return View(viewModel);
        }
        //public IActionResult Create()
        //{
        //    ViewBag.PersonelList = new SelectList(_unitOfWork.Personel.GetAll(), "Id", "Adi");
        //    ViewBag.MalzemeList = new SelectList(_unitOfWork.Malzeme.GetAll(), "Id", "Adi");
        //    return View();
        //}
        // GET: TasinmazBagisci/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var tasinmazBagisci = await _unitOfWork.TasinmazBagisci.GetFirstOrDefaultAsync(m=> m.Id==id, includeProperties:"Malzeme,Personel,MalzemeYeriTanim");
            if (tasinmazBagisci == null)
                return NotFound();
            var sosyalGuvenceList = new List<SelectListItem> {
              new SelectListItem { Text = ProjectConstants.SOSYALGUVENCE_YOK, Value = ProjectConstants.SOSYALGUVENCE_YOK },
              new SelectListItem { Text = ProjectConstants.SOSYALGUVENCE_SGK, Value = ProjectConstants.SOSYALGUVENCE_SGK },
              new SelectListItem { Text = ProjectConstants.SOSYALGUVENCE_EMEKLISANDIGI, Value = ProjectConstants.SOSYALGUVENCE_EMEKLISANDIGI },
              new SelectListItem { Text = ProjectConstants.SOSYALGUVENCE_SSK, Value = ProjectConstants.SOSYALGUVENCE_SSK },
              new SelectListItem { Text = ProjectConstants.SOSYALGUVENCE_BAGKUR, Value = ProjectConstants.SOSYALGUVENCE_BAGKUR }
            };
            var sagVefatList = new List<SelectListItem> {
              new SelectListItem { Text = ProjectConstants.BAGISCI_SAG, Value = ProjectConstants.BAGISCI_SAG },
              new SelectListItem { Text = ProjectConstants.BAGISCI_VEFAT, Value = ProjectConstants.BAGISCI_VEFAT },
              new SelectListItem { Text = ProjectConstants.BAGISCI_BILINMIYOR, Value = ProjectConstants.BAGISCI_BILINMIYOR },
              new SelectListItem { Text = ProjectConstants.BAGISCI_MULGA, Value = ProjectConstants.BAGISCI_MULGA },
              new SelectListItem { Text = ProjectConstants.BAGISCI_KURULUS, Value = ProjectConstants.BAGISCI_KURULUS },
              new SelectListItem { Text = ProjectConstants.BAGISCI_TASFIYE, Value = ProjectConstants.BAGISCI_TASFIYE },
              new SelectListItem { Text = ProjectConstants.BAGISCI_LAGV, Value = ProjectConstants.BAGISCI_LAGV }
            };

            var viewModel = new TasinmazBagisciVM
            {
                TasinmazBagisci = tasinmazBagisci,
                SosyalGuvenceList = sosyalGuvenceList,
                SagVefatList = sagVefatList,
                IlVeIlceVM = new IlVeIlceVM
                {
                    Iller = (await _unitOfWork.Il.GetAllAsync())
                .Select(il => new SelectListItem { Value = il.Id.ToString(), Text = il.IlAdi })
                .ToList(),
                    IlLabel = "İkamet İli",
                    IlceLabel = "İkamet İlçesi"
                }
            };
            return View(viewModel);
        }

        #endregion

        #region POST Index Create Edit Delete
        
        // POST: TasinmazBagisci/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TasinmazBagisciVM model)
        {
            if (ModelState.IsValid)
            {
                string? userName = HttpContext?.User?.Identity?.Name;
                model.TasinmazBagisci.IlId = model.IlVeIlceVM.SelectedIlId;
                model.TasinmazBagisci.IlceId = model.IlVeIlceVM.SelectedIlceId;
                model.TasinmazBagisci.Olusturan = userName;

                await _unitOfWork.TasinmazBagisci.AddAsync(model.TasinmazBagisci);
               
                await _unitOfWork.CommitAsync();
                TempData["success"] = "TasinmazBagisci işlemi gerçekleşti";
                return View(model);
                //return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["error"] = "TasinmazBagisci işlemi yapılamadı";
                // Hatalı durumda tekrar dropdownları dolduralım
                model.IlVeIlceVM.Iller = (await _unitOfWork.Il.GetAllAsync())
                    .Select(il => new SelectListItem { Value = il.Id.ToString(), Text = il.IlAdi })
                    .ToList();
                model.IlVeIlceVM.Ilceler = new List<SelectListItem>();

                return View(model);
            }
        }
        // POST: TasinmazBagisci/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TasinmazBagisciVM tasinmazBagisciVM)
        {
            //MalzemeDagilim'da mevcut malzemeId'nin adedini mevcut Çıkış Yerine ekle
            //MalzemeDagilim'da yeni adedi, yeni çıkış yerinden düş
            if (ModelState.IsValid)
            {
                
            }
            else
            {
                TempData["error"] = "TasinmazBagisci işlemi yapılamadı";
            }
            return View(tasinmazBagisciVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var entity = await _unitOfWork.TasinmazBagisci.GetFirstOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                {
                    return NotFound();
                }
                //TODO SB Bu bağışçının bağışı varsa silinmesin
                _unitOfWork.TasinmazBagisci.Remove(entity);
                await _unitOfWork.CommitAsync(); // Save changes to the database

                TempData["SuccessMessage"] = "Bağışçı başarıyla silind.";
                return RedirectToAction("Index"); // Redirect to the list page after deletion
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Bağışçı silinemedi.";
                // Log the exception (using a logging framework like Serilog, NLog, etc.)
                return RedirectToAction("Index");
            }
        }
        #endregion

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var list = _unitOfWork.TasinmazBagisci.GetAll();

            return Json(new { data = list });
        }
        
        
        #endregion
    }


}
