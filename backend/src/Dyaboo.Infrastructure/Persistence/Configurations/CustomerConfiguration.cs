using Dyaboo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dyaboo.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.Name).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Type).IsRequired();
        builder.Property(c => c.ContactName).HasMaxLength(150);
        builder.Property(c => c.Phone).HasMaxLength(30);
        builder.Property(c => c.City).HasMaxLength(100);

        builder.HasIndex(c => c.Name);
    }
}
