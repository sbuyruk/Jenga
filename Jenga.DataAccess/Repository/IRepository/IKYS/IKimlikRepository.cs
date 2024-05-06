using Jenga.Models.IKYS;


namespace Jenga.DataAccess.Repository.IRepository.IKYS
{
    public interface IKimlikRepository : IRepository<Kimlik>
    {
        void Update(Kimlik obj);

    }
}
