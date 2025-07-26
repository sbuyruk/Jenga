using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Ortak;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class LokasyonController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public LokasyonController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> GetIlveIlceDropdown(int? selectedIlId, int? selectedIlceId)
        {
            var iller = await _unitOfWork.Il.GetAllAsync();
            var ilceler = selectedIlId.HasValue
                ? await _unitOfWork.Ilce.GetByIlIdAsync(selectedIlId.Value)
                : new List<Ilce>();

            var model = new IlVeIlceVM
            {
                SelectedIlId = selectedIlId ?? 0,
                SelectedIlceId = selectedIlceId,
                Iller = iller.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.IlAdi }).ToList(),
                Ilceler = ilceler.Select(x => new SelectListItem { Value = x.IlceId.ToString(), Text = x.IlceAdi }).ToList()
            };

            return PartialView("_IlVeIlceDropdown", model);
        }
        [HttpGet]
        public async Task<IActionResult> GetIlceByIl(int ilId)
        {
            var ilceler = await _unitOfWork.Ilce.GetByIlIdAsync(ilId);
            var result = ilceler.Select(x => new { value = x.IlceId, text = x.IlceAdi }).ToList();

            return Json(result);
        }
    }

}
