using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Ortak;
using Jenga.Models.Ortak;

namespace Jenga.DataAccess.Repositories.Ortak
{
    public class PersonelMenuRepository : Repository<PersonelMenu>, IPersonelMenuRepository
    {
        ApplicationDbContext _db;
        public PersonelMenuRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public List<PersonelMenu> GetPersonelMenuByPersonelId(int? personelId)
        {
            return _db.Set<PersonelMenu>()
                .Where(pt => pt.PersonelId == personelId)
                .ToList();
        }
    }
}
