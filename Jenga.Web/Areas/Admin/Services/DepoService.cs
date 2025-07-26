using Jenga.DataAccess.Repositories.IRepository;

namespace Jenga.Web.Areas.Admin.Services
{
    public class DepoService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DepoService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }



    }

}