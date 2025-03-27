using Microsoft.AspNetCore.Mvc;
using Jenga.Models.DYS;
using Jenga.DataAccess.Repository.IRepository;


namespace Jenga.Web.Areas.Admin.Controllers
{
    //[Area("Admin")]
    public class MalzemeOzellikController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        // GET: Admin/Malzeme
        
        public IActionResult Index()
        {

            return View();
        }
        public MalzemeOzellikController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }
        #region CreateEditDelete
        
        [HttpPost]
        public IActionResult SaveSelectedRows([FromBody] SaveRowsViewModel model1)
        {
            if (model1.SelectedIds == null || model1.SelectedIds.Count == 0)
            {
                return BadRequest("No rows selected.");
            }

            // Perform saving logic here
            // You can loop over the selectedIds to save them to the database
            foreach (var id in model1.SelectedIds)
            {
                MalzemeOzellik malzemeOzellik= _unitOfWork.MalzemeOzellik.GetFirstOrDefault(e => e.MalzemeId== model1.MalzemeId && e.OzellikId == id);
                if (malzemeOzellik == null)
                {

                    MalzemeOzellik mo = new()
                    {
                        MalzemeId = model1.MalzemeId,
                        OzellikId = id,
                    };
                    string? userName = HttpContext.User.Identity.Name;
                    mo.Olusturan = userName;
                    _unitOfWork.MalzemeOzellik.Add(mo);
                    TempData["success"] = "Özellikler Eklendi";
                }
            }
            _unitOfWork.Save();
            return Ok(new { message = "Data saved successfully" });
        }
        public class SaveRowsViewModel
        {
            public List<int> SelectedIds { get; set; }
            public int MalzemeId { get; set; }  // New field for additional data
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
        [HttpGet]
        public IActionResult GetAllByMalzemeId(int malzemeId)
        {
            List<MalzemeOzellik> list = _unitOfWork.MalzemeOzellik.GetByFilter(u => u.MalzemeId == malzemeId, includeProperties: "Malzeme,Ozellik").ToList();
            //var aa = JsonConvert.SerializeObject(list, Formatting.None,
            //            new JsonSerializerSettings()
            //            {
            //                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            //            });
            //var result = new JsonResult(JsonConvert.DeserializeObject(aa));
            //return Json(new { data = result.Value });
            return Json(new { data = list });
        }

        #endregion
        #region MalzemeOzellik Table
        [HttpDelete]
        public IActionResult DeleteRow(int id)
        {
            if (id == 0)
            {
                return BadRequest("Invalid ID.");
            }

            // Use Unit of Work or Repository to delete the record
            var entity = _unitOfWork.MalzemeOzellik.GetFirstOrDefault(u => u.Id == id);

            if (entity == null)
            {
                return NotFound(new { message = "Record not found." });
            }

            _unitOfWork.MalzemeOzellik.Remove(entity);
            _unitOfWork.Save();  // Save changes

            return Ok(new { message = "Row deleted successfully." });
        }
        #endregion
    }
}
