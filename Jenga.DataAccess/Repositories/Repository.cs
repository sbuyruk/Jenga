using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Inventory;
using Jenga.Models.Sistem;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Threading;

namespace Jenga.DataAccess.Repositories
{
    public class Repository<T> : IRepository<T> where T : BaseModel
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<T> dbSet;
        public Repository(ApplicationDbContext db)
        {
            _db = db;
            dbSet = _db.Set<T>();
        }
        public void Remove(T entity)
        {
            if (entity != null)
            {
                // Zaten track edilen entity varsa, attach etme!
                var trackedEntity = dbSet.Local.FirstOrDefault(e => e.Id == entity.Id);
                if (trackedEntity != null)
                {
                    dbSet.Remove(trackedEntity);
                }
                else
                {
                    dbSet.Attach(entity);
                    dbSet.Remove(entity);
                }
            }
        }

        public void RemoveRange(IEnumerable<T> entity)
        {
            dbSet.RemoveRange(entity);
           
        }
        //public void Update(T entity)
        //{
        //    entity.DegistirmeTarihi = DateTime.Now;
        //    dbSet.Entry(entity).Property(x => x.Id).IsModified = false;
        //    dbSet.Attach(entity);  // Attach to DbContext
        //    dbSet.Entry(entity).State = EntityState.Modified;
        //    dbSet.Update(entity);  // Mark the entity as Modified
        //}
        public void Update(T entity)
        {
            entity.Degistiren = Environment.UserName;
            entity.DegistirmeTarihi = DateTime.Now;
            dbSet.Update(entity);
        }
        //SB async update CancellationToken kısmı nanay
        //public async Task UpdateAsync(T entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
        //{
        //    entity.Degistiren = string.IsNullOrWhiteSpace(modifiedBy) ? Environment.UserName : modifiedBy;
        //    entity.DegistirmeTarihi = DateTime.Now;
        //    dbSet.Update(entity);
        //    await _db.SaveChangesAsync(cancellationToken);
        //    dbSet.Entry(entity).State = EntityState.Detached;
        //}
        public async Task UpdateAsync(T entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
        {
            // Önce context'ten var olanı bul
            var trackedEntity = await dbSet.FindAsync(entity.Id);
            if (trackedEntity != null)
            {
                // Tüm property'leri güncelle (otomatik map veya manuel kopyala)

                _db.Entry(trackedEntity).CurrentValues.SetValues(entity);
                trackedEntity.Degistiren = string.IsNullOrWhiteSpace(modifiedBy) ? Environment.UserName : modifiedBy;
                trackedEntity.DegistirmeTarihi = DateTime.Now;
            }
            else
            {
                // Eğer context'te yoksa, attach et ve update et
                dbSet.Attach(entity);
                dbSet.Update(entity);
            }

            await _db.SaveChangesAsync(cancellationToken);
        }
        //async methods
        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            entity.Olusturan = Environment.UserName;
            entity.OlusturmaTarihi = DateTime.Now;
            await dbSet.AddAsync(entity, cancellationToken);
        }
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _db.SaveChangesAsync(cancellationToken);
        }

        // SB Unlike AddAsync, the Update method doesn’t need to be awaited since it doesn’t perform asynchronous work. It simply marks the entity as modified in the context.

        public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }
        public async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await dbSet.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter, string? includeProperties = null, bool trackChanges = true)
        {
            IQueryable<T> query = dbSet;
            if (!trackChanges) //If you only need to read the data without modifying it, using AsNoTracking() is an effective way to avoid tracking the entity:
            {
                query = query.AsNoTracking();
            }
            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            query = query.Where(filter);
            return await query.FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<T>> GetAllAsync(string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;

            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            return await query.ToListAsync();
        }
        public async Task<IEnumerable<T>> GetAllByFilterAsync(Expression<Func<T, bool>> filter, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;

            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            query = query.Where(filter);
            return await query.ToListAsync();
        }
        public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return dbSet.AnyAsync(predicate);
        }
    }
}
