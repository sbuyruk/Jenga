using Jenga.Models.IKYS;

namespace Jenga.DataAccess.Repositories.IRepository.IKYS
{
    public interface IPersonelRepository : IRepository<Personel>
    {
        Task SaveAsync(CancellationToken cancellationToken = default);

    }
}