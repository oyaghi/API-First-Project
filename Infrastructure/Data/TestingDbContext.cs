using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class TestingDbContext : DbContext
    {
        public TestingDbContext(DbContextOptions<TestingDbContext> Options) : base(Options)
        {

        }

        public DbSet<User> users { get; set; }
        public DbSet<Tenant> tenants { get; set; }
    }

}

