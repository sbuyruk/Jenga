using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Ortak;
using Jenga.Models.TBYS;
using Jenga.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.SharePoint.Client;
using System.Net;

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
                DefinIliList = await _unitOfWork.Il.GetIlDDL(),

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

        // GET: TasinmazBagisci/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var tasinmazBagisci = await _unitOfWork.TasinmazBagisci.GetFirstOrDefaultAsync(m => m.Id == id);
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


                ResimYükle();
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
                ResimYükle();
                return View(model);
            }
        }

        private void ResimYükle()
        {

            var helper = new SharePointHelper();

            string siteUrl = "http://tskgv-portal";///YonetimBirimleri/InsaatVeEmlakYonetimiSubesi/"; // SharePoint site URL
            string libraryName = "BagisciResimleri";          // Resim kütüphanesi adı
            string filePath = @"C:\Home\Resim.jpg";       // Yüklenecek dosyanın yolu
            string username = "asbuyruk";            // Domain kullanıcı adı
            string password = "Tagstags777";                       // Kullanıcı parolası

            SharePointHelper.UploadFileToSharePoint(siteUrl, libraryName, filePath, username, password);
        }
        [HttpPost]
        public async Task<IActionResult> UploadCroppedImage(IFormFile croppedImage)
        {
            if (croppedImage == null || croppedImage.Length == 0)
            {
                return BadRequest("No image uploaded.");
            }

            try
            {
                string siteUrl = "http://tskgv-portal/YonetimBirimleri/InsaatVeEmlakYonetimiSubesi/";
                string username = "asbuyruk";
                string password = "Tagstags777";

                string domain = "tskgv"; // Leave empty if not using a domain

                try
                {
                    // Create network credentials
                    NetworkCredential credentials = new NetworkCredential(username, password);

                    // Connect to SharePoint
                    using (ClientContext context = new ClientContext(siteUrl))
                    {

                        context.Credentials = credentials;


                        // Access the SharePoint web
                        Microsoft.SharePoint.Client.Web web = context.Web;

                        //Read the image stream
                        using (var stream = croppedImage.OpenReadStream())
                        {
                            // Convert the stream to a byte array
                            using (var memoryStream = new MemoryStream())
                            {
                                await stream.CopyToAsync(memoryStream);
                                byte[] imageBytes = memoryStream.ToArray();

                                // Call SharePoint upload method
                                await UploadImageToSharePoint(imageBytes, croppedImage.FileName, context);
                            }
                        }

                        context.Load(web, w => w.Title);
                        context.ExecuteQuery();

                        Console.WriteLine($"Site Title: {web.Title}");
                    }

                }
                catch (MissingMethodException ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                    Console.WriteLine($"StackTrace: {ex.StackTrace}");
                }

                return Ok("Image uploaded successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        private async Task UploadImageToSharePoint(byte[] imageBytes, string fileName, ClientContext context)
        {


            var folder = context.Web.GetFolderByServerRelativeUrl("YonetimBirimleri/InsaatVeEmlakYonetimiSubesi/BagisciResimleri");

            using (var memoryStream = new MemoryStream(imageBytes))
            {
                var fileCreationInfo = new FileCreationInformation
                {
                    ContentStream = memoryStream,
                    Url = fileName,
                    Overwrite = true
                };

                var uploadFile = folder.Files.Add(fileCreationInfo);
                context.Load(uploadFile);
                await context.ExecuteQueryAsync();
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> UploadCroppedImage(IFormFile croppedImage)
        //{

        //    if (croppedImage != null && croppedImage.Length > 0)
        //    {
        //        var networkPath = @"http://tskgv-dev3/YonetimBirimleri/InsaatVeEmlakYonetimiSubesi/BagisciResimleri/";//@"\\TSKGV-DEV3\home\images";
        //        var fileName = $"{Guid.NewGuid()}.png";
        //        var filePath = Path.Combine(networkPath, fileName);

        //        try
        //        {
        //            if (!Directory.Exists(networkPath))
        //            {
        //                Directory.CreateDirectory(networkPath);
        //            }

        //            using (var stream = new FileStream(filePath, FileMode.Create))
        //            {
        //                await croppedImage.CopyToAsync(stream);
        //            }

        //            return Json(new { success = true, message = "Image uploaded successfully." });
        //        }
        //        catch (Exception ex)
        //        {
        //            return Json(new { success = false, message = $"Error: {ex.Message}" });
        //        }
        //    }
        //    return Json(new { success = false, message = "No file selected." });
        //}

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
