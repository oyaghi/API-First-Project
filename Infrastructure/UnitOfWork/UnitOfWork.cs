using Core.IRepository;
using Core.IUnitOfWork;
using Core.Models;
using Infrastructure.Repository;
using Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly TestingDbContext _context;

    public IRepository<User> Users { get; private set; }

    public UnitOfWork(TestingDbContext context)
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
