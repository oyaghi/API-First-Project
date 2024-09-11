using Core.IRepository;
using Core.Models;

namespace Core.IUnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<User> Users { get; }

        Task<int> SaveAsync();
    }
}
