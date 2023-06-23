using Jenga.DataAccess.Repository.IRepository;
using Jenga.Models.MTS;
using Jenga.Models.Ortak;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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