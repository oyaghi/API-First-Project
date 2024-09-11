using API_First_Project.IRepository;
using API_First_Project.Models;
using API_First_Project.Repository;
using Castle.Core.Resource;
using Microsoft.EntityFrameworkCore;
namespace API_First_Project.UnitOfWork

{
    public class UnitOfWork : IUnitOfWork.IUnitOfWorks
    {
        private readonly DbContext _context;

        public IRepository<User> Users { get; private set; }

        public UnitOfWork(DbContext context)
        {
            _context = context;
            Users = new Repository<User>(_context);

        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
