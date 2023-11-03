using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.MTS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting;
using Jenga.Models.Ortak;
using Jenga.Models.IKYS;
using Jenga.DataAccess.Repository;
using Jenga.Web.Areas.Admin.Services;
using System.Linq;

namespace Jenga.Web.Areas.Admin.Controllers
{
    public class PersonelMenuController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly MenuService _menuService;
        public PersonelMenuController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment, MenuService menuService)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
            _menuService = menuService;
        }
        #region GET
        public IActionResult Index()
        {
            return View();
        }

        //GET
        public IActionResult Create(int? id)
        {

            PersonelMenuVM personelMenuVM = new()
            {

                PersonelMenu = new(),
                MenuTanimSelecList = _unitOfWork.MenuTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
                Personel = _unitOfWork.Personel.GetFirstOrDefault(u => u.Id == id)
            };

            return View(personelMenuVM);


        }
        public IActionResult Edit(int? id)
        {
            Personel personel = _unitOfWork.Personel.GetFirstOrDefault(u => u.Id == id);
            List<MenuTanim> assignedMenuList = _menuService.GetMenuListByPersonelId(personel.Id);
            List<MenuTanimVM> menuTanimList = _menuService.GetMenuTanimVmListByParent(1);
            PersonelMenuVM personelMenuVM = new()
            {
                PersonelMenu = new(),
                MenuTanimSelecList = _unitOfWork.MenuTanim.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Adi,
                    Value = i.Id.ToString()
                }),
                Personel = _unitOfWork.Personel.GetFirstOrDefault(u => u.Id == id),
                MenuTanimList = menuTanimList,
                SelectedMenuTanimList = assignedMenuList
            };

            if (id == null || id == 0)
            {
                return NotFound();

            }
            else
            {
                //update 
                personelMenuVM.PersonelMenu = _unitOfWork.PersonelMenu.GetFirstOrDefault(u => u.PersonelId == id);
                return View(personelMenuVM);
            }

        }       
        #endregion
        #region POST
        //Delete
        [HttpPost]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _unitOfWork.PersonelMenu.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Kayıt silmede hata" });
            }

            _unitOfWork.PersonelMenu.Remove(obj);
            PersonelMenuVM personelMenuVM = new()
            {
                PersonelMenu = obj

            };
            _unitOfWork.Save();
            return Json(new { success = true, message = "Yetki silindi" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PersonelMenuVM obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.PersonelMenu.Id == 0)
                {
                    string? userName = HttpContext.User.Identity.Name;
                    obj.Personel.Olusturan = userName;
                    _unitOfWork.PersonelMenu.Add(obj.PersonelMenu);
                    TempData["success"] = "Menu işlemi gerçekleşti";
                }
                else
                {
                    TempData["error"] = "PersonelMenu Id bulunamadı";
                }

                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(PersonelMenuVM obj)
        {
            if (obj!=null && obj.Personel != null && obj.Personel.Id > 0)
            {
                Personel personel = _unitOfWork.Personel.GetFirstOrDefault(u => u.Id == obj.Personel.Id);
                List<MenuTanim> assignedMenuList = _menuService.GetMenuListByPersonelId(personel.Id);
                string[]? selectedMenuIds = (obj == null ? string.Empty : obj.SelectedMenuTanimString)?.Split(",");

                // checked olanlar
                if (selectedMenuIds != null)
                {
                    foreach (var item in selectedMenuIds)
                    {
                        MenuTanim menuTanim = _unitOfWork.MenuTanim.GetFirstOrDefault(m => m.Id == int.Parse(item));
                        PersonelMenu personelMenu = _unitOfWork.PersonelMenu.GetFirstOrDefault(pm => pm.MenuTanimId == int.Parse(item) && pm.PersonelId == obj.Personel.Id);
                        if (personelMenu != null) //update et
                        {
                            //personelMenu.MenuTanimId = menuTanim.Id;

                            //_unitOfWork.PersonelMenu.Update(personelMenu);
                            //TempData["success"] = "Yetki işlemi güncellendi";

                        }
                        else //insert et
                        {
                            PersonelMenu personelMenuNew = new()
                            {
                                MenuTanimId = menuTanim.Id,
                                PersonelId = personel.Id,
                                Aciklama="Eklendi",
                                Olusturan="ben",
                                OlusturmaTarihi= DateTime.Now,
                            };
                            string? userName = HttpContext.User.Identity.Name;
                            obj.Personel.Olusturan = userName;
                            _unitOfWork.PersonelMenu.Add(personelMenuNew);
                            TempData["success"] = "Yetki işlemi güncellendi";
                        }
                    }
                    //bir de cheked iken unchecked olanlar varsa loop içinde onlara bakalım
                    foreach (var item in assignedMenuList)
                    {
                        if (item.Id>1 && !selectedMenuIds.ToList().Contains(item.Id.ToString())) //bu demekki silindi
                        {
                            MenuTanim menuTanim = _unitOfWork.MenuTanim.GetFirstOrDefault(m => m.Id == item.Id);
                            PersonelMenu personelMenu = _unitOfWork.PersonelMenu.GetFirstOrDefault(pm => pm.MenuTanimId == item.Id && pm.PersonelId == personel.Id);
                            // bulduk mu
                            if (personelMenu != null)
                            {
                                _unitOfWork.PersonelMenu.Remove(personelMenu);
                            }
                        }
                    }
                }
                _unitOfWork.Save();
            }
            else
            {
                TempData["error"] = "PersonelMenu Id bulunamadı";
            }
            return RedirectToAction("Index");
        } 
        #endregion
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var personelMenuList = _unitOfWork.PersonelMenu.GetAll(includeProperties: "Personel,MenuTanim");
            return Json(new { data = personelMenuList });
        }
        public IActionResult GetPersonelAll()
        {
            var personelMenus = _unitOfWork.PersonelMenu.GetAll(includeProperties: "Personel,MenuTanim");
            //var personel = _unitOfWork.Personel.GetAll();
            var personel = _unitOfWork.IsBilgileri.GetAll(includeProperties: "Personel").Where(ib=>ib.CalismaDurumu.Equals("1"));

            var personelMenuList = personel
                .GroupJoin(personelMenus,
                    p => p.Id,
                    pm => pm.PersonelId,
                    (p, pm) => new {
                        Personel = p,
                        MenuTanim = string.Join(", ", pm.Select(t => t.MenuTanim.Adi))
                    });

            return Json(new { data = personelMenuList });
        }
        #endregion

    }
}