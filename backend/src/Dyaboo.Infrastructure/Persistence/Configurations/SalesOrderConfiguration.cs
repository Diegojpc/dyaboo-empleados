using Dyaboo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dyaboo.Infrastructure.Persistence.Configurations;

public class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("sales_orders");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();

        builder.Property(o => o.OrderCode).IsRequired().HasMaxLength(50);
        builder.HasIndex(o => o.OrderCode).IsUnique();

        builder.Property(o => o.Status).IsRequired();
        builder.Property(o => o.Notes).HasMaxLength(500);

        builder.Ignore(o => o.Total);
        builder.Ignore(o => o.TotalUnits);

        builder.HasIndex(o => o.Status);

        builder.HasOne(o => o.Customer)
            .WithMany()
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Items)
            .WithOne(i => i.SalesOrder)
            .HasForeignKey(i => i.SalesOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
