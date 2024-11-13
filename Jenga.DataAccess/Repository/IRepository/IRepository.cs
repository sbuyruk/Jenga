using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Jenga.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {


        List<T> GetByFilter(Expression<Func<T,bool>> filter, string? includeProperties = null);
        T GetFirstOrDefault(Expression<Func<T,bool>> filter, string? includeProperties = null);
        IEnumerable<T> GetAll(string? includeProperties = null);
        void Add(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entity);
        void Update(T entity); //SB
        //async interfaces //SB
        Task AddAsync(T entity);
        Task<T?> GetAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter, string? includeProperties = null, bool trackChanges = true);
        Task<IEnumerable<T>> GetAllAsync(string? includeProperties = null);

    }
}
