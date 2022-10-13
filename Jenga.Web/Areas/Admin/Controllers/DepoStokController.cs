using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models;
using Jenga.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class DepoStokController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public DepoStokController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }
        
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var depoStokList = _unitOfWork.DepoStok.GetAll(includeProperties:"DepoTanim,AniObjesiTanim");
            return Json(new { data = depoStokList });
        }

        #endregion
    }


}
