using Jenga.DataAccess.Repository;
using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.IKYS;
using Jenga.Models.MTS;
using Jenga.Utility;
using Microsoft.EntityFrameworkCore;


namespace Jenga.Web.Areas.Admin.Services
{
    public class FaaliyetService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FaaliyetService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


    }

}