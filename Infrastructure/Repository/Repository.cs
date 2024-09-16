using Core.IRepository;
using Core.IUnitOfWork;
using Core.MetaData;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;


namespace Infrastructure.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly DbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(DbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAsync( Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null!)
        {
            IQueryable<T> query = _dbSet;

            // Apply pagination
          
            if (orderBy != null)
            {
                query = orderBy(query);
            }

          

            return await query.ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filter, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
        {
            IQueryable<T> query = _dbSet.Where(filter);

           
            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return await query.ToListAsync();
        }

        public async Task<T?> FindSingleAsync(Expression<Func<T, bool>> filter)
        {
            var query = await _dbSet.Where(filter).SingleOrDefaultAsync();
            return query;
        }

        public async Task<PagedResult<T>> GetPagedAsync(int pageNumber,int pageSize)
        {
            IQueryable<T> query = _dbSet.AsNoTracking();

            int rowCount = await query.CountAsync();

            var pagedData = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            int pageCount = (int)Math.Ceiling(rowCount / (double)pageSize);

            var result = new PagedResult<T>
            {
                Results = pagedData,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                RowCount = rowCount,
                PageCount = pageCount
            };

            return result;
        }
    }
}


