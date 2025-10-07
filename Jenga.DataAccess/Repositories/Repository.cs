using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Inventory;
using Jenga.Models.Sistem;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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

        public void Add(T entity)
        {
            entity.Olusturan = Environment.UserName;
            entity.OlusturmaTarihi = DateTime.Now;
            dbSet.Add(entity);
        }
        //IncludeProp - "KaynakTanim, DepoTanim, vs"
        public IEnumerable<T> GetAll(string? includeProperties = null)
        {
            try
            {
                IQueryable<T> query = dbSet;

                if (includeProperties != null)
                {
                    foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        query = query.Include(includeProp);
                    }
                }
                return query.ToList();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null)
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
            return query.FirstOrDefault();
        }
        public List<T> GetByFilter(Expression<Func<T, bool>> filter, string? includeProperties = null)
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
            return query.ToList();
        }
        public void Remove(T entity)
        {
            if (entity != null)
            {
                dbSet.Remove(entity);
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
            entity.Degistiren= Environment.UserName;
            entity.DegistirmeTarihi = DateTime.Now;
            dbSet.Update(entity);
        }
        //SB async update CancellationToken kısmı nanay
        public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            entity.Degistiren = Environment.UserName;
            entity.DegistirmeTarihi = DateTime.Now;
            dbSet.Update(entity);
            await Task.CompletedTask;
        }

        //async methods
        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {   entity.Olusturan = Environment.UserName;
            entity.OlusturmaTarihi = DateTime.Now;
            await dbSet.AddAsync(entity, cancellationToken);
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

    }
}
