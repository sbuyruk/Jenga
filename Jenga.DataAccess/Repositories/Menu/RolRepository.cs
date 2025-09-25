using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.Menu;
using Jenga.Models.Common;
using Microsoft.EntityFrameworkCore;


namespace Jenga.DataAccess.Repositories.Menu
{

    public class RolRepository : Repository<Rol>, IRolRepository
    {
        ApplicationDbContext _db;
        public RolRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }
        public async Task<List<Rol>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            //return await _db.Set<Rol>()
            //                     .AsNoTracking()
            //                     .ToListAsync(cancellationToken);
            var result= await _db.Rol_Table.ToListAsync(cancellationToken);
            return result;
        }
        public async Task<Rol?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _db.Set<Rol>()
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        }
        public async Task AddAsync(Rol rol, CancellationToken cancellationToken = default)
        {
            await _db.Set<Rol>().AddAsync(rol, cancellationToken);
        }

        public async Task<Rol?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _db.Set<Rol>()
                .Include(r => r.PersonelRoller)
                    .ThenInclude(pr => pr.Personel)
                .Include(r => r.RolMenuleri)
                    .ThenInclude(rm => rm.Menu)
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        }
    }
}
