using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Common;

namespace Jenga.DataAccess.Services.Common
{
    public class IlceService : IIlceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private List<Ilce>? _cache;

        public IlceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Ilce>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            _cache ??= await _unitOfWork.Ilce.GetAllAsync(cancellationToken);
            return _cache;
        }

        public Task<Ilce?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => _unitOfWork.Ilce.GetByIdAsync(id, cancellationToken);

        public Task<List<Ilce>> GetByIlIdAsync(int ilId, CancellationToken cancellationToken = default)
            => _unitOfWork.Ilce.GetByIlIdAsync(ilId, cancellationToken);
        
        public Task<List<Ilce>> GetAktifIlcelerAsync(CancellationToken cancellationToken = default)
            => _unitOfWork.Ilce.GetAktifIlcelerAsync(cancellationToken);
    }
}
