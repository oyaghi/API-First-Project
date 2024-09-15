using Core.Models;
using Infrastructure.Services.TenantIdGetter;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class TestingDbContext : DbContext
    {
        private readonly ITenantService _tenantService;

        // Store the tenant ID in a private field
        private int _tenantId;

        public TestingDbContext(DbContextOptions<TestingDbContext> options, ITenantService tenantService) : base(options)
        {
            _tenantService = tenantService;
            // Set the tenant ID when the context is created
            _tenantId = _tenantService.GetTenantId();
        }

        public DbSet<User> users { get; set; }
        public DbSet<Tenant> tenants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Apply the global query filter using the stored tenant ID
            modelBuilder.Entity<User>().HasQueryFilter(u => _tenantId == 0 || u.TenantId == _tenantId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
