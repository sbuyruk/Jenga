using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository.IKYS;
using Jenga.Models.IKYS;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Jenga.DataAccess.Repositories.IKYS
{
    // PersonelRepository updated to accept IDbContextFactory and use short-lived DbContext instances.
    public class PersonelRepository : Repository<Personel>, IPersonelRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public PersonelRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : base(dbFactory)
        {
            _dbFactory = dbFactory;
        }

        // If you previously had a sync Save() that used a long-lived DbContext, prefer an async SaveAsync when using factory-created contexts.
        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            await db.SaveChangesAsync(cancellationToken);
        }

        // Returns SelectListItems; uses a short-lived context for the Zimmet lookup to avoid sharing a DbContext across awaits.
        public async Task<IEnumerable<SelectListItem>> GetPersonelDDL(bool onlyWorkingPersonel = true, int malzemeId = 0, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();

            // Base repository no longer exposes dbSet as a field, so use db.Set<Personel>()
            IQueryable<Personel> query = db.Set<Personel>();

            if (onlyWorkingPersonel)
            {
                // Adjust the predicate if IsBilgileri can be null
                query = query.Where(u => u.IsBilgileri != null && u.IsBilgileri.CalismaDurumu == "1");
            }

            var personelList = await query
               .Select(p => new
               {
                   p.Id,
                   Adi = p.Adi + " " + p.Soyadi,
                   AdetSum = db.Zimmet_Table.Where(z => z.PersonelId == p.Id && (malzemeId == 0 || z.MalzemeId == malzemeId))
                       .Sum(z => (int?)z.Adet) ?? 0
               })
               .ToListAsync(cancellationToken);

            var personelDropdownList = personelList.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Adi + (m.AdetSum > 0 ? " (" + m.AdetSum + ") " : string.Empty)
            }).ToList();

            return personelDropdownList;
        }
    }
}