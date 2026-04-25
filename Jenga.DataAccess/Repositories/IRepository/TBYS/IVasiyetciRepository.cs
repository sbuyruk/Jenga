using Jenga.Models.TBYS;

namespace Jenga.DataAccess.Repositories.IRepository.TBYS
{
    public interface IVasiyetciRepository : IRepository<Vasiyetci>
    {
        Task<List<Vasiyetci>> GetByTCKimlikAsync(long tcKimlik, CancellationToken cancellationToken = default);
    }
}
