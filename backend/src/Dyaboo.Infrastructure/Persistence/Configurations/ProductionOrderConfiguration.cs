using Dyaboo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dyaboo.Infrastructure.Persistence.Configurations;

public class ProductionOrderConfiguration : IEntityTypeConfiguration<ProductionOrder>
{
    public void Configure(EntityTypeBuilder<ProductionOrder> builder)
    {
        builder.ToTable("production_orders");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();

        builder.Property(o => o.OrderCode).IsRequired().HasMaxLength(50);
        builder.HasIndex(o => o.OrderCode).IsUnique();

        builder.Property(o => o.Status).IsRequired();
        builder.Property(o => o.OverheadPercentage)
            .HasColumnType("numeric(5,2)")
            .IsRequired();

        // Propiedades calculadas: ignoradas en BD (computed en memoria)
        builder.Ignore(o => o.TotalMaterialCost);
        builder.Ignore(o => o.TotalLaborCost);
        builder.Ignore(o => o.TotalOverheadCost);
        builder.Ignore(o => o.GrandTotal);
        builder.Ignore(o => o.TotalUnits);

        builder.HasOne(o => o.ProductReference)
            .WithMany()
            .HasForeignKey(o => o.ProductReferenceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Items)
            .WithOne(i => i.ProductionOrder)
            .HasForeignKey(i => i.ProductionOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
