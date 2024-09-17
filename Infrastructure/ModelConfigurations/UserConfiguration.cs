using Core.Enums;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace Infrastructure.ModelConfigurations
{
    internal class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Email)
                   .IsRequired()
                   .HasMaxLength(255)
                   .HasAnnotation("EmailAddress", true);

            builder.Property(u => u.FirstName)
                   .IsRequired()
                   .HasMaxLength(155);

            builder.Property(u => u.Lastname)
                   .IsRequired();

            builder.Property(u => u.Gender)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasAnnotation("EnumDataType", typeof(Gender));

            builder.Property(u => u.PhoneNumber)
                   .IsRequired()
                   .HasMaxLength(10)
                   .HasAnnotation("Phone", true);

            builder.HasOne(u => u.Tenant)
                   .WithMany(t => t.Users)
                   .HasForeignKey(u => u.TenantId)
                   .OnDelete(DeleteBehavior.Cascade);     
        }
    }
}