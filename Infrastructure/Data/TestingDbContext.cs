using Castle.Core.Resource;
using Core;
using Core.Enums;
using Core.Models;
using Infrastructure.ModelConfigurations;
using Infrastructure.Services.TenantIdGetter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // Json Column 
            configurationBuilder.Properties<List<string>>().HaveConversion<StringListConverter>();
            
            base.ConfigureConventions(configurationBuilder);
        }

        private class StringListConverter : ValueConverter<List<string>, string> 
        {
            public StringListConverter():
                base(v=> string.Join(", ", v!),
                    v=> v.Split(',', StringSplitOptions.TrimEntries).ToList())
            {

            }
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Tenant> Tenants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Global Query filter 
            modelBuilder.Entity<User>().HasQueryFilter(u => u.TenantId == _tenantId);

            // User Config file 
            modelBuilder.ApplyConfiguration(new UserConfiguration());

            // Config User.Setting Json column
            modelBuilder.Entity<User>().OwnsOne(u => u.Setting, options =>
            options.ToJson());
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