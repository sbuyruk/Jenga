using Jenga.Models.DYS;

namespace Jenga.DataAccess.Repositories.IRepository.DYS
{
    public interface IMalzemeDagilimRepository : IRepository<MalzemeDagilim>
    {
        IEnumerable<MalzemeDagilim> GetMalzemeDagilimWithDetails();

    }
}
