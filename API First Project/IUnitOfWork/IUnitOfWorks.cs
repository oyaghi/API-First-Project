using API_First_Project.IRepository;
using API_First_Project.Models;
using Castle.Core.Resource;

namespace API_First_Project.IUnitOfWork
{
    public interface IUnitOfWorks: IDisposable
    {
        IRepository<User> Users { get; }
       
        Task<int> CompleteAsync();  // To save changes
    }
}
