using API_First_Project.Data;
using API_First_Project.IRepository;
using API_First_Project.IUnitOfWork;
using API_First_Project.Models;
using API_First_Project.Repository;

public class UnitOfWork : IUnitOfWorks
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
