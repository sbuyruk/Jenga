using Jenga.DataAccess.Data;
using Jenga.DataAccess.Repositories.IRepository;
using Jenga.Models.Sistem;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Jenga.DataAccess.Repositories
{
    // Repository artık IDbContextFactory ile çalışır.
    // Her çağrıda kısa ömürlü bir DbContext yaratılır -> aynı context üzerinde eş zamanlı işlemler engellenir.
    public class Repository<T> : IRepository<T> where T : BaseModel
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public Repository(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        // Senkron Remove artık aynı context içinde commit ediyor (SaveChanges çağrısı).
        public void Remove(T entity)
        {
            if (entity == null) return;

            using var db = _dbFactory.CreateDbContext();
            var dbSet = db.Set<T>();

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

            // Commit hemen yapılır
            db.SaveChanges();
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            using var db = _dbFactory.CreateDbContext();
            var dbSet = db.Set<T>();
            dbSet.RemoveRange(entities);
            db.SaveChanges();
        }

        public void Update(T entity)
        {
            entity.Degistiren = Environment.UserName;
            entity.DegistirmeTarihi = DateTime.Now;

            using var db = _dbFactory.CreateDbContext();
            var dbSet = db.Set<T>();
            dbSet.Update(entity);
            db.SaveChanges();
        }

        public async Task UpdateAsync(T entity, string? modifiedBy = null, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            var dbSet = db.Set<T>();

            var trackedEntity = await dbSet.FindAsync(new object[] { entity.Id }, cancellationToken);
            if (trackedEntity != null)
            {
                db.Entry(trackedEntity).CurrentValues.SetValues(entity);
                trackedEntity.Degistiren = string.IsNullOrWhiteSpace(modifiedBy) ? Environment.UserName : modifiedBy;
                trackedEntity.DegistirmeTarihi = DateTime.Now;
            }
            else
            {
                dbSet.Attach(entity);
                dbSet.Update(entity);
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        // AddAsync şimdi ekledikten sonra aynı context üzerinde SaveChangesAsync çağırır.
        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            entity.Olusturan = Environment.UserName;
            entity.OlusturmaTarihi = DateTime.Now;

            await using var db = _dbFactory.CreateDbContext();
            var dbSet = db.Set<T>();
            await dbSet.AddAsync(entity, cancellationToken);

            // commit burada yapılır, böylece ekleme eklediği aynı context üzerinde persist olur
            await db.SaveChangesAsync(cancellationToken);
        }

        // Bu metot hâlen kısa ömürlü context üzerinde SaveChanges çağırır; artık çoğu mutasyon metodu kendi commit'ini yaptığı için
        // çağıran kodun tekrar SaveChanges çağırmasına gerek yok (ancak çağrı varsa da hata vermez).
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.Set<T>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.Set<T>().AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter, string? includeProperties = null, bool trackChanges = true)
        {
            await using var db = _dbFactory.CreateDbContext();
            IQueryable<T> query = db.Set<T>();
            if (!trackChanges)
                query = query.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProp in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            return await query.Where(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(string? includeProperties = null)
        {
            await using var db = _dbFactory.CreateDbContext();
            IQueryable<T> query = db.Set<T>();
            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProp in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllByFilterAsync(Expression<Func<T, bool>> filter, string? includeProperties = null)
        {
            await using var db = _dbFactory.CreateDbContext();
            IQueryable<T> query = db.Set<T>();
            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProp in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            query = query.Where(filter);
            return await query.ToListAsync();
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            await using var db = _dbFactory.CreateDbContext();
            return await db.Set<T>().AnyAsync(predicate, cancellationToken);
        }
    }
}