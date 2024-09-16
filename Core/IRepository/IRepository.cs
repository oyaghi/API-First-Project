using System.Linq.Expressions;

namespace Core.IRepository
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);

        Task<IEnumerable<T>> GetAsync(Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null!);

        Task AddAsync(T entity);

        void Update(T entity);

        void Delete(T entity);

        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filter, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);

        Task<T?> FindSingleAsync(Expression<Func<T, bool>> filter);

    }
}