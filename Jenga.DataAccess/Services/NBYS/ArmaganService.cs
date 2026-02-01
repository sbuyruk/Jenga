using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.NBYS;

namespace Jenga.DataAccess.Services.NBYS
{
    public class ArmaganService : IArmaganService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ArmaganService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<List<Armagan>> GetAllAsync(CancellationToken cancellationToken = default)
            => _unitOfWork.Armagan.GetAllAsync(cancellationToken);

        public Task<Armagan?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => _unitOfWork.Armagan.GetByIdAsync(id, cancellationToken);

        public async Task<bool> AddAsync(Armagan model, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.Armagan.AddAsync(model, cancellationToken);
            await _unitOfWork.Armagan.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAsync(Armagan model, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.Armagan.UpdateAsync(model, null, cancellationToken);
            await _unitOfWork.Armagan.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(Armagan model, CancellationToken cancellationToken = default)
        {
            _unitOfWork.Armagan.Remove(model);
            await _unitOfWork.Armagan.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
