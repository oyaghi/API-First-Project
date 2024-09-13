using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    /*
     * DbContext
     * bridge between your domain or entity classes and the database. 
     * It is responsible for managing the entity objects during runtime
     * including retrieving them from the database, tracking changes to those objects, and persisting those changes back to the database.
     */

    public class TestingDbContext : DbContext
    {
        public TestingDbContext(DbContextOptions<TestingDbContext> Options) : base(Options)
        {

        }

        public DbSet<User> users { get; set; }
        public DbSet<Tenant> tenants { get; set; }

        // Lazy loading Config 
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseLazyLoadingProxies();
            }
        }


    }

}

