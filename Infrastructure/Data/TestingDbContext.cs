using Core;
using Core.Models;
using Infrastructure.Services.TenantIdGetter;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class TestingDbContext : DbContext
    {
        private readonly ITenantService _tenantService;
        private int _tenantId;

        public TestingDbContext(DbContextOptions<TestingDbContext> options, ITenantService tenantService) : base(options)
        {
            _tenantService = tenantService;
            _tenantId = _tenantService.GetTenantId();
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Tenant> Tenants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasQueryFilter(u => u.TenantId == _tenantId);

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            SetTenantId();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetTenantId();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void SetTenantId()
        {
            int tenantId = _tenantService.GetTenantId();

            // Handle the Created/Added records
            var newEntities = ChangeTracker.Entries<ITenantEntity>()
                .Where(e => e.State == EntityState.Added);

            foreach (var entity in newEntities)
            {
                entity.Entity.TenantId = tenantId;
            }

          
            // Handle the Updated/Modified records 
            var modifiedEntities = ChangeTracker.Entries<ITenantEntity>()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entity in modifiedEntities)
            {
                var originalTenantId = (int)entity.OriginalValues[nameof(ITenantEntity.TenantId)]!;

                if (entity.Entity.TenantId != originalTenantId)
                {
                    entity.Entity.TenantId = originalTenantId;
                }
            }
        }

    }
}
