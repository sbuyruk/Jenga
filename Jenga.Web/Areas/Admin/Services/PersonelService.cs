using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.IKYS;
using Microsoft.EntityFrameworkCore;

namespace Jenga.Web.Areas.Admin.Services
{
    public class PersonelService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PersonelService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Çalışan Personel listesi</returns>
        public List<Personel> GetCalisanPersonelList()
        {
            var list = _unitOfWork.Personel.GetAll(includeProperties: "IsBilgileri").ToList();
            return list;
        }

    }

}