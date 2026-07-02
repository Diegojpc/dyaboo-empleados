using Dyaboo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dyaboo.Infrastructure.Persistence.Configurations;

public class CuttingOrderConfiguration : IEntityTypeConfiguration<CuttingOrder>
{
    public void Configure(EntityTypeBuilder<CuttingOrder> builder)
    {
        builder.ToTable("cutting_orders");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();

        builder.Property(o => o.OrderCode).IsRequired().HasMaxLength(50);
        builder.HasIndex(o => o.OrderCode).IsUnique();

        builder.Property(o => o.Status).IsRequired();
        builder.Property(o => o.Notes).HasMaxLength(500);

        builder.Ignore(o => o.TotalPlannedUnits);
        builder.Ignore(o => o.TotalCutUnits);

        builder.HasOne(o => o.ProductReference)
            .WithMany()
            .HasForeignKey(o => o.ProductReferenceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Items)
            .WithOne(i => i.CuttingOrder)
            .HasForeignKey(i => i.CuttingOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
